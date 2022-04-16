using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IContainerCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ContainerCodeBuilder : RangeCodeBaseBuilder, IContainerCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ContainerResolution _containerResolution;
    private readonly IReadOnlyList<ITransientScopeCodeBuilder> _transientScopeCodeBuilders;
    private readonly IReadOnlyList<IScopeCodeBuilder> _scopeCodeBuilders;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDictionary(StringBuilder stringBuilder)
    {
        if (_containerResolution.DisposalType is DisposalType.None)
            return stringBuilder;

        var dictionaryTypeName = _containerResolution.DisposalType is DisposalType.Async
            ? WellKnownTypes.ConcurrentDictionaryOfAsyncDisposable.FullName()
            : WellKnownTypes.ConcurrentDictionaryOfSyncDisposable.FullName();

        stringBuilder.AppendLine(
            $"private {dictionaryTypeName} {_containerResolution.TransientScopeDisposalReference} = new {dictionaryTypeName}();");
        
        return stringBuilder;
    }

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDisposal(StringBuilder stringBuilder)
    {
        if (_containerResolution.DisposalType is DisposalType.None)
            return stringBuilder;

        var elementName = _containerResolution.TransientScopeDisposalElement;

        var asyncSuffix = _containerResolution.DisposalType is DisposalType.Async ? "Async" : "";
        var awaitPrefix = _containerResolution.DisposalType is DisposalType.Async ? "await " : "";

        stringBuilder
            .AppendLine($"while ({_containerResolution.TransientScopeDisposalReference}.Count > 0)")
            .AppendLine($"{{")
            .AppendLine($"var {elementName} = global::System.Linq.Enumerable.FirstOrDefault({_containerResolution.TransientScopeDisposalReference}.Keys);")
            .AppendLine($"if ({elementName} is not null && {_containerResolution.TransientScopeDisposalReference}.TryRemove({elementName}, out _))")
            .AppendLine($"{{")
            .AppendLine($"{awaitPrefix}{elementName}.Dispose{asyncSuffix}();")
            .AppendLine($"}}")
            .AppendLine($"}}")
            .AppendLine($"{_containerResolution.TransientScopeDisposalReference}.Clear();");

        return stringBuilder;
    }

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        var disposableImplementation = _containerResolution.DisposalType switch
        {
            DisposalType.Sync => $" : {WellKnownTypes.Disposable.FullName()}",
            DisposalType.Async => $" : {WellKnownTypes.AsyncDisposable.FullName()}",
            _ => ""
        };
        
        stringBuilder = stringBuilder
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name}{disposableImplementation}")
            .AppendLine($"{{");

        stringBuilder = GenerateResolutionRange(
            stringBuilder,
            _containerResolution);
        
        stringBuilder = stringBuilder
            .AppendLine($"internal interface {_containerResolution.TransientScopeInterface.Name}")
            .AppendLine($"{{");

        stringBuilder = _containerResolution.TransientScopeInterface.Functions.Aggregate(
            stringBuilder,
            (sb, f) => sb.AppendLine(
                $"{SelectFullName(f)} {f.Reference}({string.Join(", ", f.Parameter.Select(p => $"{p.TypeFullName} {p.Reference}"))});"));
        
        stringBuilder = stringBuilder
            .AppendLine($"}}");

        stringBuilder = stringBuilder
            .AppendLine($"internal class {_containerResolution.TransientScopeInterface.ContainerAdapterName} : {_containerResolution.TransientScopeInterface.Name}")
            .AppendLine($"{{")
            .AppendLine($"private {_containerInfo.FullName} _container;")
            .AppendLine($"internal {_containerResolution.TransientScopeInterface.ContainerAdapterName}({_containerInfo.FullName} container) => _container = container;");

        stringBuilder = _containerResolution.TransientScopeInterface.Functions.Aggregate(
            stringBuilder,
            (sb, f) => sb.AppendLine(
                $"public {SelectFullName(f)} {f.Reference}({string.Join(", ", f.Parameter.Select(p => $"{p.TypeFullName} {p.Reference}"))}) =>")
                .AppendLine($"_container.{f.Reference}({string.Join(", ", f.Parameter.Select(p => p.Reference))});"));
        
        stringBuilder = stringBuilder
            .AppendLine($"}}")
            .AppendLine($"private {_containerResolution.TransientScopeInterface.ContainerAdapterName}? _{_containerResolution.TransientScopeAdapterReference};")
            .AppendLine($"private {_containerResolution.TransientScopeInterface.ContainerAdapterName} {_containerResolution.TransientScopeAdapterReference} => _{_containerResolution.TransientScopeAdapterReference} ??= new {_containerResolution.TransientScopeInterface.ContainerAdapterName}(this);");

        stringBuilder = _transientScopeCodeBuilders.Aggregate(stringBuilder, (sb, cb) => cb.Build(sb));
        
        stringBuilder = _scopeCodeBuilders.Aggregate(stringBuilder, (sb, cb) => cb.Build(sb));

        return stringBuilder
            .AppendLine($"}}")
            .AppendLine($"}}")
            .AppendLine($"#nullable disable");

        string SelectFullName(InterfaceFunctionDeclarationResolution interfaceResolution) =>
            interfaceResolution.SynchronicityDecision.Value switch
            {
                SynchronicityDecision.AsyncValueTask => interfaceResolution.ValueTaskTypeFullName,
                SynchronicityDecision.AsyncTask => interfaceResolution.TaskTypeFullName,
                SynchronicityDecision.Sync => interfaceResolution.TypeFullName,
                _ => throw new ArgumentException("Synchronicity not decided for the interface function at time of generating the sources")
            };
    }

    public ContainerCodeBuilder(
        // parameter
        IContainerInfo containerInfo,
        ContainerResolution containerResolution,
        IReadOnlyList<ITransientScopeCodeBuilder> transientScopeCodeBuilders,
        IReadOnlyList<IScopeCodeBuilder> scopeCodeBuilders,
        
        // dependencies
        WellKnownTypes wellKnownTypes) 
        : base(containerResolution, containerResolution, wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _containerResolution = containerResolution;
        _transientScopeCodeBuilders = transientScopeCodeBuilders;
        _scopeCodeBuilders = scopeCodeBuilders;
    }
}