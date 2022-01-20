namespace MrMeeseeks.DIE.CodeBuilding;

internal interface ITransientScopeCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class TransientScopeCodeBuilder : RangeCodeBaseBuilder, ITransientScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly TransientScopeResolution _transientScopeResolution;
    private readonly ContainerResolution _containerResolution;

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        if (!_transientScopeResolution.RootResolutions.Any() && !_transientScopeResolution.RangedInstances.Any()) 
            return stringBuilder;
        
        stringBuilder = stringBuilder
            .AppendLine(
                $"internal partial class {_transientScopeResolution.Name} : {_containerResolution.TransientScopeInterface.Name}, {WellKnownTypes.Disposable.FullName()}")
            .AppendLine($"{{")
            .AppendLine($"private readonly {_containerInfo.FullName} {_transientScopeResolution.ContainerReference};")
            .AppendLine($"internal {_transientScopeResolution.Name}(")
            .AppendLine($"{_containerInfo.FullName} {_transientScopeResolution.ContainerParameterReference})")
            .AppendLine($"{{")
            .AppendLine($"{_transientScopeResolution.ContainerReference} = {_transientScopeResolution.ContainerParameterReference};")
            .AppendLine($"}}");

        stringBuilder = GenerateResolutionRange(
            stringBuilder,
            _transientScopeResolution);

        stringBuilder = stringBuilder
            .AppendLine($"}}");

        return stringBuilder;
    }

    public TransientScopeCodeBuilder(
        // parameter
        IContainerInfo containerInfo,
        TransientScopeResolution transientScopeResolution,
        ContainerResolution containerResolution,
        
        // dependencies
        WellKnownTypes wellKnownTypes) 
        : base(transientScopeResolution, containerResolution, wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _transientScopeResolution = transientScopeResolution;
        _containerResolution = containerResolution;
    }
}