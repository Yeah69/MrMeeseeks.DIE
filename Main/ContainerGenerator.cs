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

            var generatedContainer = new StringBuilder()
                .AppendLine($"namespace {containerInfo.Namespace}")
                .AppendLine($"{{")
                .AppendLine($"partial class {containerInfo.Name} : {_wellKnownTypes.Disposable.FullName()}")
                .AppendLine($"{{");

            generatedContainer = GenerateContainerDisposalFunction(
                generatedContainer,
                containerResolution.DisposalHandling,
                containerResolution);
            
            foreach (var singleInstanceResolution in containerResolution.SingleInstanceResolutions)
            {
                generatedContainer = generatedContainer
                    .AppendLine($"private {singleInstanceResolution.Function.TypeFullName} {singleInstanceResolution.Function.FieldReference};")
                    .AppendLine($"private {_wellKnownTypes.SemaphoreSlim.FullName()} {singleInstanceResolution.Function.LockReference} = new {_wellKnownTypes.SemaphoreSlim.FullName()}(1);")
                    .AppendLine($"public {singleInstanceResolution.Function.TypeFullName} {singleInstanceResolution.Function.Reference}()")
                    .AppendLine($"{{")
                    .AppendLine($"if (!object.ReferenceEquals({singleInstanceResolution.Function.FieldReference}, null)) return {singleInstanceResolution.Function.FieldReference};")
                    .AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Wait();")
                    .AppendLine($"try")
                    .AppendLine($"{{")
                    .AppendLine($"if (this.{containerResolution.DisposalHandling.DisposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(nameof({containerInfo.Name}));");
            
                generatedContainer = GenerateResolutionFunction(generatedContainer, singleInstanceResolution.Dependency);

                generatedContainer = generatedContainer
                    .AppendLine($"this.{singleInstanceResolution.Function.FieldReference} = {singleInstanceResolution.Dependency.Reference};")
                    .AppendLine($"}}")
                    .AppendLine($"finally")
                    .AppendLine($"{{")
                    .AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Release();")
                    .AppendLine($"}}")
                    .AppendLine($"return this.{singleInstanceResolution.Function.FieldReference};")
                    .AppendLine($"}}");
            }
            
            generatedContainer = generatedContainer
                .AppendLine($"public {containerResolution.TypeFullName} Resolve()")
                .AppendLine($"{{")
                .AppendLine($"if (this.{containerResolution.DisposalHandling.DisposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(nameof({containerInfo.Name}));");
            
            generatedContainer = GenerateResolutionFunction(generatedContainer, containerResolution);

            generatedContainer = generatedContainer
                .AppendLine($"return {containerResolution.Reference};")
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

            
            StringBuilder GenerateContainerDisposalFunction(
                StringBuilder stringBuilder,
                ContainerResolutionDisposalHandling disposalHandling,
                ContainerResolution containerResolution)
            {
                stringBuilder = stringBuilder
                    .AppendLine($"private {disposalHandling.DisposableCollection.TypeFullName} {disposalHandling.DisposableCollection.Reference} = new {disposalHandling.DisposableCollection.TypeFullName}();")
                    .AppendLine($"private int {disposalHandling.DisposedFieldReference} = 0;")
                    .AppendLine($"private bool {disposalHandling.DisposedPropertyReference} => {disposalHandling.DisposedFieldReference} != 0;")
                    .AppendLine($"public void Dispose()")
                    .AppendLine($"{{")
                    .AppendLine($"var {disposalHandling.DisposedLocalReference} = global::System.Threading.Interlocked.Exchange(ref this.{disposalHandling.DisposedFieldReference}, 1);")
                    .AppendLine($"if ({disposalHandling.DisposedLocalReference} != 0) return;");

                foreach (var singleInstanceResolution in containerResolution.SingleInstanceResolutions)
                {
                    stringBuilder = stringBuilder
                        .AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Wait();");

                }
                
                stringBuilder = stringBuilder
                    .AppendLine($"try")
                    .AppendLine($"{{")
                    .AppendLine($"foreach(var {disposalHandling.DisposableLocalReference} in {disposalHandling.DisposableCollection.Reference})")
                    .AppendLine($"{{")
                    .AppendLine($"try")
                    .AppendLine($"{{")
                    .AppendLine($"{disposalHandling.DisposableLocalReference}.Dispose();")
                    .AppendLine($"}}")
                    .AppendLine($"catch({_wellKnownTypes.Exception.FullName()})")
                    .AppendLine($"{{")
                    .AppendLine($"// catch and ignore exceptions of individual disposals so the other disposals are triggered")
                    .AppendLine($"}}")
                    .AppendLine($"}}")
                    .AppendLine($"}}")
                    .AppendLine($"finally")
                    .AppendLine($"{{");

                foreach (var singleInstanceResolution in containerResolution.SingleInstanceResolutions)
                {
                    stringBuilder = stringBuilder
                        .AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Release();");
                }
                
                stringBuilder = stringBuilder
                    .AppendLine($"}}")
                    .AppendLine($"}}");

                return stringBuilder;
            }

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
                    case SingleInstanceReferenceResolution(var reference, { TypeFullName: {} typeFullName}):
                        stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};"); 
                        break;
                    case ContainerResolution(var rootResolution, _, _):
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
                    case SingleInstanceReferenceResolution(var reference, { Reference: {} functionReference}):
                        stringBuilder = stringBuilder.AppendLine($"{reference} = {functionReference}();"); 
                        break;
                    case ContainerResolution(var rootResolution, _, _):
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
