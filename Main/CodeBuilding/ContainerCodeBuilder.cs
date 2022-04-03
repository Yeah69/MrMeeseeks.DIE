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

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        stringBuilder = stringBuilder
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name} : {WellKnownTypes.Disposable.FullName()}")
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