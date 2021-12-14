namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IContainerCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ContainerCodeBuilder : RangeCodeBaseBuilder, IContainerCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ContainerResolution _containerResolution;
    private readonly IScopeCodeBuilder _defaultScopeBuilder;

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        stringBuilder = stringBuilder
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name} : {WellKnownTypes.Disposable.FullName()}")
            .AppendLine($"{{");

        stringBuilder = GenerateResolutionRange(
            stringBuilder,
            _containerResolution);

        stringBuilder = _defaultScopeBuilder.Build(stringBuilder);

        return stringBuilder
            .AppendLine($"}}")
            .AppendLine($"}}");
    }

    public ContainerCodeBuilder(
        // parameter
        IContainerInfo containerInfo,
        ContainerResolution containerResolution,
        IScopeCodeBuilder defaultScopeBuilder,
        
        // dependencies
        WellKnownTypes wellKnownTypes) 
        : base(wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _containerResolution = containerResolution;
        _defaultScopeBuilder = defaultScopeBuilder;
    }
}