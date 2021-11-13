using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MrMeeseeks.DIE;

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
        if (!containerInfo.IsValid)
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

        generatedContainer = containerResolution.SingleInstanceResolutions.Aggregate(generatedContainer, GenerateRangedInstanceFunction);
        generatedContainer = containerResolution.ScopedInstanceResolutions.Aggregate(generatedContainer, GenerateRangedInstanceFunction);

        generatedContainer = containerResolution.RootResolutions.Aggregate(generatedContainer, GenerateResolutionFunction)
            .AppendLine($"}}")
            .AppendLine($"}}");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);
        
        StringBuilder GenerateContainerDisposalFunction(
            StringBuilder stringBuilder,
            DisposalHandling disposalHandling,
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

            stringBuilder = containerResolution.SingleInstanceResolutions.Aggregate(
                stringBuilder, 
                (current, singleInstanceResolution) => current.AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Wait();"));

            stringBuilder = containerResolution.ScopedInstanceResolutions.Aggregate(
                stringBuilder, 
                (current, singleInstanceResolution) => current.AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Wait();"));

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
                
            return stringBuilder
                .AppendLine($"}}")
                .AppendLine($"}}");
        }

        StringBuilder GenerateResolutionFunction(
            StringBuilder stringBuilder,
            (Resolvable, INamedTypeSymbol) resolution)
        {
            var (resolvable, type) = resolution;
            stringBuilder = stringBuilder
                .AppendLine($"{resolvable.TypeFullName} {_wellKnownTypes.Container.Construct(type).FullName()}.Resolve()")
                .AppendLine($"{{")
                .AppendLine($"if (this.{containerResolution.DisposalHandling.DisposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(nameof({containerInfo.Name}));");
            
            return GenerateResolutionFunctionContent(stringBuilder, resolvable)
                .AppendLine($"return {resolvable.Reference};")
                .AppendLine($"}}");
        }

        StringBuilder GenerateResolutionFunctionContent(
            StringBuilder stringBuilder,
            Resolvable resolution)
        {
            stringBuilder = GenerateFields(stringBuilder, resolution);
            return GenerateResolutions(stringBuilder, resolution);
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
                case ScopedInstanceReferenceResolution(var reference, { TypeFullName: {} typeFullName}):
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};"); 
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
                case ScopedInstanceReferenceResolution(var reference, { Reference: {} functionReference}):
                    stringBuilder = stringBuilder.AppendLine($"{reference} = {functionReference}();"); 
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
                    GenerateResolutionFunctionContent(stringBuilder, resolutionBase);
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

        StringBuilder GenerateRangedInstanceFunction<T>(StringBuilder stringBuilder, RangedInstanceBase<T> rangedInstance) 
            where T : RangedInstanceFunctionBase
        {
            stringBuilder = stringBuilder
                .AppendLine(
                    $"private {rangedInstance.Function.TypeFullName} {rangedInstance.Function.FieldReference};")
                .AppendLine(
                    $"private {_wellKnownTypes.SemaphoreSlim.FullName()} {rangedInstance.Function.LockReference} = new {_wellKnownTypes.SemaphoreSlim.FullName()}(1);")
                .AppendLine(
                    $"public {rangedInstance.Function.TypeFullName} {rangedInstance.Function.Reference}()")
                .AppendLine($"{{")
                .AppendLine(
                    $"if (!object.ReferenceEquals({rangedInstance.Function.FieldReference}, null)) return {rangedInstance.Function.FieldReference};")
                .AppendLine($"this.{rangedInstance.Function.LockReference}.Wait();")
                .AppendLine($"try")
                .AppendLine($"{{")
                .AppendLine(
                    $"if (this.{containerResolution.DisposalHandling.DisposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(nameof({containerInfo.Name}));");

            stringBuilder = GenerateResolutionFunctionContent(stringBuilder, rangedInstance.Dependency);

            stringBuilder = stringBuilder
                .AppendLine(
                    $"this.{rangedInstance.Function.FieldReference} = {rangedInstance.Dependency.Reference};")
                .AppendLine($"}}")
                .AppendLine($"finally")
                .AppendLine($"{{")
                .AppendLine($"this.{rangedInstance.Function.LockReference}.Release();")
                .AppendLine($"}}")
                .AppendLine($"return this.{rangedInstance.Function.FieldReference};")
                .AppendLine($"}}");
            return stringBuilder;
        }
    }
}