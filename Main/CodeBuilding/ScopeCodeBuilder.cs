namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IScopeCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ScopeCodeBuilder : RangeCodeBaseBuilder, IScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ScopeResolution _scopeResolution;

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        if (!_scopeResolution.RootResolutions.Any() && !_scopeResolution.RangedInstances.Any()) 
            return stringBuilder;
        
        stringBuilder = stringBuilder
            .AppendLine(
                $"internal partial class {_scopeResolution.Name} : {WellKnownTypes.Disposable.FullName()}")
            .AppendLine($"{{")
            .AppendLine($"private readonly {_containerInfo.FullName} {_scopeResolution.ContainerReference};")
            .AppendLine(
                $"internal {_scopeResolution.Name}({_containerInfo.FullName} {_scopeResolution.ContainerParameterReference})")
            .AppendLine($"{{")
            .AppendLine(
                $"{_scopeResolution.ContainerReference} = {_scopeResolution.ContainerParameterReference};")
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
        
        // dependencies
        WellKnownTypes wellKnownTypes) 
        : base(wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _scopeResolution = scopeResolution;
    }
}