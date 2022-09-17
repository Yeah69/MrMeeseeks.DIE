using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface ITransientScopeCodeBuilder : IRangeCodeBaseBuilder
{
    public TransientScopeResolution TransientScopeResolution { get; }
}

internal class TransientScopeCodeBuilder : RangeCodeBaseBuilder, ITransientScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ContainerResolution _containerResolution;
    
    public TransientScopeResolution TransientScopeResolution { get; }

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDictionary(StringBuilder stringBuilder) => stringBuilder;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDisposal(StringBuilder stringBuilder)
    {
        if (_containerResolution.DisposalType is DisposalType.None)
            return stringBuilder;

        var disposalType = _containerResolution.DisposalType.HasFlag(DisposalType.Async) 
            ? WellKnownTypes.AsyncDisposable.FullName() 
            : WellKnownTypes.Disposable.FullName();

        stringBuilder
            .AppendLine($"{TransientScopeResolution.ContainerReference}.{_containerResolution.TransientScopeDisposalReference}.TryRemove(({disposalType}) this, out _);");

        return stringBuilder;
    }

    protected override bool ExplicitTransientScopeInstanceImplementation => true;

    public StringBuilder BuildFunction(StringBuilder stringBuilder, Func<StringBuilder, StringBuilder> functionResolution)
    {
        stringBuilder = stringBuilder
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name}")
            .AppendLine($"{{");
        
        stringBuilder = stringBuilder
            .AppendLine($"private partial class {TransientScopeResolution.Name}")
            .AppendLine($"{{");

        stringBuilder = functionResolution(stringBuilder);

        stringBuilder = stringBuilder
            .AppendLine($"}}");
        
        return stringBuilder
            .AppendLine($"}}")
            .AppendLine($"}}")
            .AppendLine($"#nullable disable");
    }

    public override StringBuilder BuildGeneral(StringBuilder stringBuilder)
    {
        var disposableImplementation = _containerResolution.DisposalType.HasFlag(DisposalType.Async)
            ? $", {WellKnownTypes.AsyncDisposable.FullName()}"
            : $", {WellKnownTypes.AsyncDisposable.FullName()}, {WellKnownTypes.Disposable.FullName()}";
        
        stringBuilder = stringBuilder
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name}")
            .AppendLine($"{{");
        
        stringBuilder = stringBuilder
            .AppendLine($"private partial class {TransientScopeResolution.Name} : {_containerResolution.TransientScopeInterface.Name}{disposableImplementation}")
            .AppendLine($"{{")
            .AppendLine($"private readonly {_containerInfo.FullName} {TransientScopeResolution.ContainerReference};")
            .AppendLine($"internal {TransientScopeResolution.Name}(")
            .AppendLine($"{_containerInfo.FullName} {TransientScopeResolution.ContainerParameterReference})")
            .AppendLine($"{{")
            .AppendLine($"{TransientScopeResolution.ContainerReference} = {TransientScopeResolution.ContainerParameterReference};")
            .AppendLine($"}}");

        stringBuilder = GenerateResolutionRange(
            stringBuilder,
            TransientScopeResolution);

        stringBuilder = stringBuilder
            .AppendLine($"}}");
        
        return stringBuilder
            .AppendLine($"}}")
            .AppendLine($"}}")
            .AppendLine($"#nullable disable");
    }

    public override StringBuilder BuildCreateFunction(StringBuilder stringBuilder, FunctionResolution functionResolution) => 
        BuildFunction(stringBuilder, sb => GenerateResolutionFunction(sb, functionResolution));

    public override StringBuilder BuildRangedFunction(StringBuilder stringBuilder,
        RangedInstanceFunctionGroupResolution rangedInstanceFunctionGroupResolution) =>
        BuildFunction(stringBuilder, sb => GenerateRangedInstanceFunction(sb, rangedInstanceFunctionGroupResolution));

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
        TransientScopeResolution = transientScopeResolution;
        _containerResolution = containerResolution;
    }
}