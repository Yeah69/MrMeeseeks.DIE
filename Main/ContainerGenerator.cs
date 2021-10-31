using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace MrMeeseeks.DIE
{
    internal interface IContainerGenerator 
    {
        void Generate(IContainerInfo containerInfo, ContainerResolution resolvable);
    }

    internal class ContainerGenerator : IContainerGenerator
    {
        private readonly GeneratorExecutionContext _context;
        private readonly WellKnownTypes _wellKnownTypes;
        private readonly IDiagLogger _diagLogger;

        public ContainerGenerator(
            GeneratorExecutionContext context,
            WellKnownTypes wellKnownTypes,
            IDiagLogger diagLogger)
        {
            _context = context;
            _wellKnownTypes = wellKnownTypes;
            _diagLogger = diagLogger;
        }

        public void Generate(IContainerInfo containerInfo, ContainerResolution containerResolution)
        {
            if (!containerInfo.IsValid || containerInfo.ResolutionRootType is null)
            {
                _diagLogger.Log($"return generation");
                return;
            }

            var funcName = _wellKnownTypes.Func.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType |
                                  SymbolDisplayParameterOptions.IncludeParamsRefOut,
                memberOptions: SymbolDisplayMemberOptions.IncludeRef));
            
            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace {containerInfo.Namespace}")
                .AppendLine($"{{")
                .AppendLine($"partial class {containerInfo.Name}")
                .AppendLine($"{{")
                .AppendLine($"public TResult Run<TResult, TParam>({funcName}<{containerResolution.TypeFullName}, TParam, TResult> func, TParam param)")
                .AppendLine($"{{")
                .AppendLine($"TResult ret;");
            
            generatedContainer = GenerateResolutionFunction(generatedContainer, containerResolution.DisposableCollection);
                
            generatedContainer = generatedContainer
                .AppendLine($"try")
                .AppendLine($"{{");
            
            generatedContainer = GenerateResolutionFunction(generatedContainer, containerResolution);

            generatedContainer = generatedContainer
                .AppendLine($"ret = func({containerResolution.Reference}, param);")
                .AppendLine($"}}")
                .AppendLine($"finally")
                .AppendLine($"{{")
                .AppendLine($"foreach(var disposable in {containerResolution.DisposableCollection.Reference})")
                .AppendLine($"{{")
                .AppendLine($"try")
                .AppendLine($"{{")
                .AppendLine($"disposable.Dispose();")
                .AppendLine($"}}")
                .AppendLine($"catch({_wellKnownTypes.Exception.FullName()})")
                .AppendLine($"{{")
                .AppendLine($"// catch and ignore exceptions of individual disposals so the other disposals are triggered")
                .AppendLine($"}}")
                .AppendLine($"}}")
                .AppendLine($"}}")
                .AppendLine($"return ret;")
                .AppendLine($"}}")
                .AppendLine($"}}")
                .AppendLine($"}}")
                ;

            var containerSource = CSharpSyntaxTree
                    .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
                    .GetRoot()
                    .NormalizeWhitespace()
                    .SyntaxTree
                    .GetText();
            _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);



            StringBuilder GenerateResolutionFunction(
                StringBuilder stringBuilder,
                Resolvable resolution)
            {
                stringBuilder = GenerateFields(stringBuilder, resolution);
                stringBuilder = GenerateResolutions(stringBuilder, resolution);
                
                return stringBuilder;
            }

            StringBuilder GenerateFields(
                StringBuilder stringBuilder,
                Resolvable resolution)
            {
                switch (resolution)
                {
                    case ContainerResolution(var rootResolution, _):
                        stringBuilder = GenerateFields(stringBuilder, rootResolution);
                        break;
                    case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                        stringBuilder = GenerateFields(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, _, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                        break;
                    case FuncResolution(var reference, var typeFullName, _, _):
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                        break;
                    case FuncParameterResolution:
                        break;
                    case CollectionResolution(var reference, var typeFullName, _, var items):
                        stringBuilder = items.OfType<Resolvable>().Aggregate(stringBuilder, GenerateFields);
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }

            StringBuilder GenerateResolutions(
                StringBuilder stringBuilder,
                Resolvable resolution)
            {
                switch (resolution)
                {
                    case ContainerResolution(var rootResolution, var disposableCollectionResolution):
                        stringBuilder = GenerateResolutions(stringBuilder, rootResolution);
                        break;
                    case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                        stringBuilder = GenerateResolutions(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var disposableCollectionResolution, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateResolutions(builder, tuple.Dependency ?? throw new Exception()));
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = new {typeFullName}({string.Join(", ", parameters.Select(d => $"{d.name}: {d.Dependency?.Reference}"))});");
                        if (disposableCollectionResolution is {})
                            stringBuilder = stringBuilder.AppendLine(
                                $"{disposableCollectionResolution.Reference}.Add(({_wellKnownTypes.Disposable.FullName()}) {reference});");
                        break;
                    case FuncResolution(var reference, _, var parameter, Resolvable resolutionBase):
                        stringBuilder = stringBuilder.AppendLine($"{reference} = ({string.Join(", ", parameter.Select(fpr => fpr.Reference))}) =>");
                        stringBuilder = stringBuilder.AppendLine($"{{");
                        GenerateResolutionFunction(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine($"return {resolutionBase.Reference};");
                        stringBuilder = stringBuilder.AppendLine($"}};");
                        break;
                    case FuncParameterResolution:
                        break;
                    case CollectionResolution(var reference, _, var itemFullName, var items):
                        stringBuilder = items.OfType<Resolvable>().Aggregate(stringBuilder, GenerateResolutions);
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = new {itemFullName}[]{{{string.Join(", ", items.Select(d => $"({itemFullName}) {(d as Resolvable)?.Reference}"))}}};");
                        break;
                    default:
                        throw new Exception("Unexpected case or not implemented.");
                }

                return stringBuilder;
            }
        }
    }
}
