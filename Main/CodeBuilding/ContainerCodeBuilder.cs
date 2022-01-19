namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IContainerCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ContainerCodeBuilder : RangeCodeBaseBuilder, IContainerCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ContainerResolution _containerResolution;
    private readonly ITransientScopeCodeBuilder _defaultTransientScopeCodeBuilder;
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
            .AppendLine($"private {_containerResolution.TransientScopeInterface.ContainerAdapterName} _{_containerResolution.TransientScopeAdapterReference};")
            .AppendLine($"private {_containerResolution.TransientScopeInterface.ContainerAdapterName} {_containerResolution.TransientScopeAdapterReference} => _{_containerResolution.TransientScopeAdapterReference} ??= new {_containerResolution.TransientScopeInterface.ContainerAdapterName}(this);");

        stringBuilder = _defaultTransientScopeCodeBuilder.Build(stringBuilder);
        
        stringBuilder = _defaultScopeBuilder.Build(stringBuilder);

        return stringBuilder
            .AppendLine($"}}")
            .AppendLine($"}}");
    }

    public ContainerCodeBuilder(
        // parameter
        IContainerInfo containerInfo,
        ContainerResolution containerResolution,
        ITransientScopeCodeBuilder defaultTransientScopeCodeBuilder,
        IScopeCodeBuilder defaultScopeBuilder,
        
        // dependencies
        WellKnownTypes wellKnownTypes) 
        : base(wellKnownTypes)
    {
        _containerInfo = containerInfo;
        _containerResolution = containerResolution;
        _defaultTransientScopeCodeBuilder = defaultTransientScopeCodeBuilder;
        _defaultScopeBuilder = defaultScopeBuilder;
    }
}