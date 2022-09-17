using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IScopeCodeBuilder : IRangeCodeBaseBuilder
{
    ScopeResolution ScopeResolution { get; }
}

internal class ScopeCodeBuilder : RangeCodeBaseBuilder, IScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly TransientScopeInterfaceResolution _transientScopeInterfaceResolution;
    private readonly ContainerResolution _containerResolution;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDictionary(StringBuilder stringBuilder) => stringBuilder;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDisposal(StringBuilder stringBuilder) => stringBuilder;

    protected override bool ExplicitTransientScopeInstanceImplementation => false;

    public StringBuilder BuildFunction(StringBuilder stringBuilder, Func<StringBuilder, StringBuilder> functionResolution)
    {
        stringBuilder = stringBuilder
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name}")
            .AppendLine($"{{");
        
        stringBuilder = stringBuilder
            .AppendLine($"private partial class {ScopeResolution.Name}")
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
            ? $" : {WellKnownTypes.AsyncDisposable.FullName()}"
            : $" : {WellKnownTypes.AsyncDisposable.FullName()}, {WellKnownTypes.Disposable.FullName()}";
        
        stringBuilder = stringBuilder
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {_containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {_containerInfo.Name}")
            .AppendLine($"{{");
        
        stringBuilder = stringBuilder
            .AppendLine(
                $"private partial class {ScopeResolution.Name}{disposableImplementation}")
            .AppendLine($"{{")
            .AppendLine($"private readonly {_containerInfo.FullName} {ScopeResolution.ContainerReference};")
            .AppendLine($"private readonly {_transientScopeInterfaceResolution.Name} {ScopeResolution.TransientScopeReference};")
            .AppendLine($"internal {ScopeResolution.Name}(")
            .AppendLine($"{_containerInfo.FullName} {ScopeResolution.ContainerParameterReference},")
            .AppendLine($"{_transientScopeInterfaceResolution.Name} {ScopeResolution.TransientScopeParameterReference})")
            .AppendLine($"{{")
            .AppendLine($"{ScopeResolution.ContainerReference} = {ScopeResolution.ContainerParameterReference};")
            .AppendLine($"{ScopeResolution.TransientScopeReference} = {ScopeResolution.TransientScopeParameterReference};")
            .AppendLine($"}}");

        stringBuilder = GenerateResolutionRange(
            stringBuilder,
            ScopeResolution);

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
        ScopeResolution = scopeResolution;
        _transientScopeInterfaceResolution = transientScopeInterfaceResolution;
        _containerResolution = containerResolution;
    }

    public ScopeResolution ScopeResolution { get; }
}