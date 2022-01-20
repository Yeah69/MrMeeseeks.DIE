namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IScopeCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ScopeCodeBuilder : RangeCodeBaseBuilder, IScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ScopeResolution _scopeResolution;
    private readonly TransientScopeInterfaceResolution _transientScopeInterfaceResolution;

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        if (!_scopeResolution.RootResolutions.Any() && !_scopeResolution.RangedInstances.Any()) 
            return stringBuilder;
        
        stringBuilder = stringBuilder
            .AppendLine(
                $"internal partial class {_scopeResolution.Name} : {WellKnownTypes.Disposable.FullName()}")
            .AppendLine($"{{")
            .AppendLine($"private readonly {_containerInfo.FullName} {_scopeResolution.ContainerReference};")
            .AppendLine($"private readonly {_transientScopeInterfaceResolution.Name} {_scopeResolution.TransientScopeReference};")
            .AppendLine($"internal {_scopeResolution.Name}(")
            .AppendLine($"{_containerInfo.FullName} {_scopeResolution.ContainerParameterReference},")
            .AppendLine($"{_transientScopeInterfaceResolution.Name} {_scopeResolution.TransientScopeParameterReference})")
            .AppendLine($"{{")
            .AppendLine($"{_scopeResolution.ContainerReference} = {_scopeResolution.ContainerParameterReference};")
            .AppendLine($"{_scopeResolution.TransientScopeReference} = {_scopeResolution.TransientScopeParameterReference};")
            .AppendLine($"}}");

        stringBuilder = GenerateResolutionRange(
            stringBuilder,
            _scopeResolution);

        stringBuilder = stringBuilder
            .AppendLine($"}}");

        return stringBuilder;
    }

    public ScopeCodeBuilder(
        // parameter
        IContainerInfo containerInfo,
        ScopeResolution scopeResolution,
        TransientScopeInterfaceResolution transientScopeInterfaceResolution,
        ContainerResolution containerResolution,
        
        // dependencies
        WellKnownTypes wellKnownTypes) 
        : base(scopeResolution, containerResolution, wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _scopeResolution = scopeResolution;
        _transientScopeInterfaceResolution = transientScopeInterfaceResolution;
    }
}