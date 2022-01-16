namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IContainerCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ContainerCodeBuilder : RangeCodeBaseBuilder, IContainerCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ContainerResolution _containerResolution;
    private readonly IScopeCodeBuilder _defaultScopeBuilder;

    protected override string TransientScopeReference { get; }

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
        
        stringBuilder = stringBuilder
            .AppendLine($"internal interface {_containerResolution.TransientScopeInterface.Name}")
            .AppendLine($"{{");

        stringBuilder = _containerResolution.TransientScopeInterface.Functions.Aggregate(
            stringBuilder,
            (sb, f) => sb.AppendLine(
                $"{f.TypeFullName} {f.Reference}({string.Join(", ", f.Parameter.Select(p => $"{p.TypeFullName} {p.Reference}"))});"));
        
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
                $"public {f.TypeFullName} {f.Reference}({string.Join(", ", f.Parameter.Select(p => $"{p.TypeFullName} {p.Reference}"))}) =>")
                .AppendLine($"_container.{f.Reference}({string.Join(", ", f.Parameter.Select(p => p.Reference))});"));
        
        stringBuilder = stringBuilder
            .AppendLine($"}}")
            .AppendLine($"private {_containerResolution.TransientScopeInterface.ContainerAdapterName} _{TransientScopeReference};")
            .AppendLine($"private {_containerResolution.TransientScopeInterface.ContainerAdapterName} {TransientScopeReference} => _{TransientScopeReference} ??= new {_containerResolution.TransientScopeInterface.ContainerAdapterName}(this);");

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

        TransientScopeReference = containerResolution.TransientScopeAdapterReference;
    }
}