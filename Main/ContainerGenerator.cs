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
        void Generate(IContainerInfo containerInfo, Resolvable resolvable);
    }

    internal class ContainerGenerator : IContainerGenerator
    {
        private readonly GeneratorExecutionContext _context;
        private readonly IDiagLogger _diagLogger;

        public ContainerGenerator(
            GeneratorExecutionContext context,
            IDiagLogger diagLogger)
        {
            _context = context;
            _diagLogger = diagLogger;
        }

        public void Generate(IContainerInfo containerInfo, Resolvable resolvable)
        {
            if (!containerInfo.IsValid || containerInfo.ResolutionRootType is null)
            {
                _diagLogger.Log($"return generation");
                return;
            }
            
            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace {containerInfo.Namespace}")
                .AppendLine($"{{")
                .AppendLine($"partial class {containerInfo.Name}")
                .AppendLine($"{{")
                .AppendLine($"public {resolvable.TypeFullName} Resolve()")
                .AppendLine($"{{");

            generatedContainer = GenerateResolutionFunction(generatedContainer, resolvable);

            generatedContainer = generatedContainer
                .AppendLine($"return {resolvable.Reference};")
                .AppendLine($"}}");

            generatedContainer = generatedContainer
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



            static StringBuilder GenerateResolutionFunction(
                StringBuilder stringBuilder,
                Resolvable resolution)
            {
                stringBuilder = GenerateFields(stringBuilder, resolution);
                stringBuilder = GenerateResolutions(stringBuilder, resolution);
                
                return stringBuilder;
            }

            static StringBuilder GenerateFields(
                StringBuilder stringBuilder,
                Resolvable resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                        stringBuilder = GenerateFields(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateFields(builder, tuple.Dependency as Resolvable ?? throw new Exception()));
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

            static StringBuilder GenerateResolutions(
                StringBuilder stringBuilder,
                Resolvable resolution)
            {
                switch (resolution)
                {
                    case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                        stringBuilder = GenerateResolutions(stringBuilder, resolutionBase);
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                        break;
                    case ConstructorResolution(var reference, var typeFullName, var parameters):
                        stringBuilder = parameters.Aggregate(stringBuilder,
                            (builder, tuple) => GenerateResolutions(builder, tuple.Dependency as Resolvable ?? throw new Exception()));
                        stringBuilder = stringBuilder.AppendLine(
                            $"{reference} = new {typeFullName}({string.Join(", ", parameters.Select(d => $"{d.name}: {(d.Dependency as Resolvable)?.Reference}"))});");
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
