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

    internal ContainerGenerator(
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

        generatedContainer = GenerateResolutionRange(
            generatedContainer,
            containerResolution);

        if (containerResolution.DefaultScope.RootResolutions.Any()
            || containerResolution.DefaultScope.RangedInstances.Any())
        {
            var defaultScopeResolution = containerResolution.DefaultScope;
            generatedContainer = generatedContainer
                .AppendLine($"internal partial class {defaultScopeResolution.Name} : {_wellKnownTypes.Disposable.FullName()}")
                .AppendLine($"{{")
                .AppendLine($"private readonly {containerInfo.FullName} {defaultScopeResolution.ContainerReference};")
                .AppendLine($"internal {defaultScopeResolution.Name}({containerInfo.FullName} {defaultScopeResolution.ContainerParameterReference})")
                .AppendLine($"{{")
                .AppendLine($"{defaultScopeResolution.ContainerReference} = {defaultScopeResolution.ContainerParameterReference};")
                .AppendLine($"}}");

            generatedContainer = GenerateResolutionRange(
                generatedContainer,
                defaultScopeResolution);
        
            generatedContainer = generatedContainer
                .AppendLine($"}}");
        }
            
        generatedContainer = generatedContainer
            .AppendLine($"}}")
            .AppendLine($"}}");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);

        StringBuilder GenerateResolutionRange(
            StringBuilder stringBuilder,
            RangeResolution rangeResolution)
        {
            stringBuilder = GenerateContainerDisposalFunction(
                stringBuilder,
                rangeResolution);

            stringBuilder = rangeResolution.RangedInstances.Aggregate(stringBuilder, GenerateRangedInstanceFunction);

            return rangeResolution.RootResolutions.Aggregate(stringBuilder, GenerateResolutionFunction);
        }
        
        StringBuilder GenerateContainerDisposalFunction(
            StringBuilder stringBuilder,
            RangeResolution rangeResolution)
        {
            var disposalHandling = rangeResolution.DisposalHandling;
            stringBuilder = stringBuilder
                .AppendLine($"private {disposalHandling.DisposableCollection.TypeFullName} {disposalHandling.DisposableCollection.Reference} = new {disposalHandling.DisposableCollection.TypeFullName}();")
                .AppendLine($"private int {disposalHandling.DisposedFieldReference} = 0;")
                .AppendLine($"private bool {disposalHandling.DisposedPropertyReference} => {disposalHandling.DisposedFieldReference} != 0;")
                .AppendLine($"public void Dispose()")
                .AppendLine($"{{")
                .AppendLine($"var {disposalHandling.DisposedLocalReference} = global::System.Threading.Interlocked.Exchange(ref this.{disposalHandling.DisposedFieldReference}, 1);")
                .AppendLine($"if ({disposalHandling.DisposedLocalReference} != 0) return;");

            stringBuilder = rangeResolution.RangedInstances.Aggregate(
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

            stringBuilder = rangeResolution.RangedInstances.Aggregate(
                stringBuilder, 
                (current, singleInstanceResolution) => current.AppendLine($"this.{singleInstanceResolution.Function.LockReference}.Release();"));

            return stringBuilder
                .AppendLine($"}}")
                .AppendLine($"}}");
        }

        StringBuilder GenerateResolutionFunction(
            StringBuilder stringBuilder,
            RootResolutionFunction resolution)
        {
            var parameter = string.Join(",", resolution.Parameter.Select(r => $"{r.TypeFullName} {r.Reference}"));
            stringBuilder = stringBuilder
                .AppendLine($"{resolution.AccessModifier} {resolution.TypeFullName} {resolution.ExplicitImplementationFullName}{(string.IsNullOrWhiteSpace(resolution.ExplicitImplementationFullName) ? "" : ".")}{resolution.Reference}({parameter})")
                .AppendLine($"{{")
                .AppendLine($"if (this.{resolution.DisposalHandling.DisposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(nameof({resolution.RangeName}));");
            
            return GenerateResolutionFunctionContent(stringBuilder, resolution.Resolvable)
                .AppendLine($"return {resolution.Resolvable.Reference};")
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
                case ScopeRootResolution(var reference, var typeFullName, var scopeReference, var scopeTypeFullName, _, _, _, _):
                    stringBuilder = stringBuilder
                        .AppendLine($"{scopeTypeFullName} {scopeReference};")
                        .AppendLine($"{typeFullName} {reference};");  
                    break;
                case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                    stringBuilder = GenerateFields(stringBuilder, resolutionBase);
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");              
                    break;
                case ConstructorResolution(var reference, var typeFullName, _, var parameters, var initializedProperties):
                    stringBuilder = parameters.Aggregate(stringBuilder,
                        (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                    stringBuilder = initializedProperties.Aggregate(stringBuilder,
                        (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                    break;
                case SyntaxValueTupleResolution(var reference, var typeFullName, var items):
                    stringBuilder = items.Aggregate(stringBuilder, GenerateFields);
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                    break;
                case FuncResolution(var reference, var typeFullName, _, _):
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                    break;
                case ParameterResolution:
                    break; // the parameter is the field
                case FieldResolution(var reference, var typeFullName, _):
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                    break;
                case FactoryResolution(var reference, var typeFullName, _, var parameters):
                    stringBuilder = parameters.Aggregate(stringBuilder,
                        (builder, tuple) => GenerateFields(builder, tuple.Dependency));
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                    break;
                case CollectionResolution(var reference, var typeFullName, _, var items):
                    stringBuilder = items.OfType<Resolvable>().Aggregate(stringBuilder, GenerateFields);
                    stringBuilder = stringBuilder.AppendLine($"{typeFullName} {reference};");
                    break;
                case var (reference, typeFullName):
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
                case ScopeRootResolution(var reference, var typeFullName, var scopeReference, var scopeTypeFullName, var singleInstanceScopeReference, var parameter, var (disposableCollectionReference, _, _, _, _), var (createFunctionReference, _)):
                    stringBuilder = stringBuilder
                        .AppendLine($"{scopeReference} = new {scopeTypeFullName}({singleInstanceScopeReference});")
                        .AppendLine($"{disposableCollectionReference}.Add(({_wellKnownTypes.Disposable.FullName()}) {scopeReference});")
                        .AppendLine($"{reference} = ({typeFullName}) {scopeReference}.{createFunctionReference}({string.Join(", ", parameter.Select(p => p.Reference))});"); 
                    break;
                case RangedInstanceReferenceResolution(var reference, { Reference: {} functionReference}, var parameter, var owningObjectReference):
                    stringBuilder = stringBuilder.AppendLine($"{reference} = {owningObjectReference}.{functionReference}({string.Join(", ", parameter.Select(p => p.Reference))});"); 
                    break;
                case InterfaceResolution(var reference, var typeFullName, Resolvable resolutionBase):
                    stringBuilder = GenerateResolutions(stringBuilder, resolutionBase);
                    stringBuilder = stringBuilder.AppendLine(
                        $"{reference} = ({typeFullName}) {resolutionBase.Reference};");              
                    break;
                case ConstructorResolution(var reference, var typeFullName, var disposableCollectionResolution, var parameters, var initializedProperties):
                    stringBuilder = parameters.Aggregate(stringBuilder,
                        (builder, tuple) => GenerateResolutions(builder, tuple.Dependency));
                    stringBuilder = initializedProperties.Aggregate(stringBuilder,
                        (builder, tuple) => GenerateResolutions(builder, tuple.Dependency));
                    var constructorParameter =
                        string.Join(", ", parameters.Select(d => $"{d.Name}: {d.Dependency.Reference}"));
                    var objectInitializerParameter = initializedProperties.Any()
                        ? $" {{ {string.Join(", ", initializedProperties.Select(p => $"{p.Name} = {p.Dependency.Reference}"))} }}"
                        : "";
                    stringBuilder = stringBuilder.AppendLine(
                        $"{reference} = new {typeFullName}({constructorParameter}){objectInitializerParameter};");
                    if (disposableCollectionResolution is {})
                        stringBuilder = stringBuilder.AppendLine(
                            $"{disposableCollectionResolution.Reference}.Add(({_wellKnownTypes.Disposable.FullName()}) {reference});");
                    break;
                case SyntaxValueTupleResolution(var reference, var typeFullName, var items):
                    stringBuilder = items.Aggregate(stringBuilder, GenerateResolutions);
                    stringBuilder = stringBuilder.AppendLine($"{reference} = ({string.Join(", ", items.Select(d => d.Reference))});");
                    break;
                case FuncResolution(var reference, _, var parameter, Resolvable resolutionBase):
                    stringBuilder = stringBuilder.AppendLine($"{reference} = ({string.Join(", ", parameter.Select(fpr => fpr.Reference))}) =>");
                    stringBuilder = stringBuilder.AppendLine($"{{");
                    GenerateResolutionFunctionContent(stringBuilder, resolutionBase);
                    stringBuilder = stringBuilder.AppendLine($"return {resolutionBase.Reference};");
                    stringBuilder = stringBuilder.AppendLine($"}};");
                    break;
                case ParameterResolution:
                    break; // parameter exists already
                case FieldResolution(var reference, _, var fieldName):
                    stringBuilder = stringBuilder.AppendLine($"{reference} = this.{fieldName};");
                    break;
                case FactoryResolution(var reference, _, var functionName, var parameters):
                    stringBuilder = parameters.Aggregate(stringBuilder,
                        (builder, tuple) => GenerateResolutions(builder, tuple.Dependency ?? throw new Exception()));
                    stringBuilder = stringBuilder.AppendLine($"{reference} = this.{functionName}({string.Join(", ", parameters.Select(t => $"{t.Name}: {t.Dependency.Reference}"))});");
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

        StringBuilder GenerateRangedInstanceFunction(StringBuilder stringBuilder, RangedInstance rangedInstance)
        {
            stringBuilder = stringBuilder
                .AppendLine(
                    $"private {rangedInstance.Function.TypeFullName} {rangedInstance.Function.FieldReference};")
                .AppendLine(
                    $"private {_wellKnownTypes.SemaphoreSlim.FullName()} {rangedInstance.Function.LockReference} = new {_wellKnownTypes.SemaphoreSlim.FullName()}(1);");
                
            foreach (var (resolvable, funcParameterResolutions) in rangedInstance.Overloads)
            {
                var parameters = string.Join(", ",
                    funcParameterResolutions.Select(p => $"{p.TypeFullName} {p.Reference}"));
                stringBuilder = stringBuilder.AppendLine(
                        $"public {rangedInstance.Function.TypeFullName} {rangedInstance.Function.Reference}({parameters})")
                    .AppendLine($"{{")
                    .AppendLine($"this.{rangedInstance.Function.LockReference}.Wait();")
                    .AppendLine($"try")
                    .AppendLine($"{{")
                    .AppendLine(
                        $"if (this.{rangedInstance.DisposalHandling.DisposedPropertyReference}) throw new {_wellKnownTypes.ObjectDisposedException}(nameof({rangedInstance.DisposalHandling.RangeName}));")
                    .AppendLine(
                        $"if (!object.ReferenceEquals({rangedInstance.Function.FieldReference}, null)) return {rangedInstance.Function.FieldReference};");

                stringBuilder = GenerateResolutionFunctionContent(stringBuilder, resolvable);

                stringBuilder = stringBuilder
                    .AppendLine(
                        $"this.{rangedInstance.Function.FieldReference} = {resolvable.Reference};")
                    .AppendLine($"}}")
                    .AppendLine($"finally")
                    .AppendLine($"{{")
                    .AppendLine($"this.{rangedInstance.Function.LockReference}.Release();")
                    .AppendLine($"}}")
                    .AppendLine($"return this.{rangedInstance.Function.FieldReference};")
                    .AppendLine($"}}");
            }
            
            return stringBuilder;
        }
    }
}