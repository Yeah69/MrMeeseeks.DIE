using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IScopeCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class ScopeCodeBuilder : RangeCodeBaseBuilder, IScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ScopeResolution _scopeResolution;
    private readonly TransientScopeInterfaceResolution _transientScopeInterfaceResolution;
    private readonly ContainerResolution _containerResolution;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDictionary(StringBuilder stringBuilder) => stringBuilder;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDisposal(StringBuilder stringBuilder) => stringBuilder;

    protected override bool ExplicitTransientScopeInstanceImplementation => false;

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        var disposableImplementation = _containerResolution.DisposalType.HasFlag(DisposalType.Async) 
                ? $" : {WellKnownTypes.AsyncDisposable.FullName()}" 
                : $" : {WellKnownTypes.AsyncDisposable.FullName()}, {WellKnownTypes.Disposable.FullName()}";
        
        stringBuilder = stringBuilder
            .AppendLine(
                $"private partial class {_scopeResolution.Name}{disposableImplementation}")
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
        _containerResolution = containerResolution;
    }
}