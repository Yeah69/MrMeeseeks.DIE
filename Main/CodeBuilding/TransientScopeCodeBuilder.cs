using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface ITransientScopeCodeBuilder : IRangeCodeBaseBuilder
{
    
}

internal class TransientScopeCodeBuilder : RangeCodeBaseBuilder, ITransientScopeCodeBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly TransientScopeResolution _transientScopeResolution;
    private readonly ContainerResolution _containerResolution;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDictionary(StringBuilder stringBuilder) => stringBuilder;

    protected override StringBuilder GenerateDisposalFunction_TransientScopeDisposal(StringBuilder stringBuilder)
    {
        if (_containerResolution.DisposalType is DisposalType.None)
            return stringBuilder;

        var disposalType = _containerResolution.DisposalType is DisposalType.Async 
            ? WellKnownTypes.AsyncDisposable.FullName() 
            : WellKnownTypes.Disposable.FullName();

        stringBuilder
            .AppendLine($"{_transientScopeResolution.ContainerReference}.{_containerResolution.TransientScopeDisposalReference}.TryRemove(({disposalType}) this, out _);");

        return stringBuilder;
    }

    public override StringBuilder Build(StringBuilder stringBuilder)
    {
        if (!_transientScopeResolution.RootResolutions.Any() && !_transientScopeResolution.RangedInstanceFunctionGroups.Any()) 
            return stringBuilder;
        
        var disposableImplementation = _containerResolution.DisposalType switch
        {
            DisposalType.Async => $", {WellKnownTypes.AsyncDisposable.FullName()}",
            _ => $", {WellKnownTypes.AsyncDisposable.FullName()}, {WellKnownTypes.Disposable.FullName()}"
        };
        
        stringBuilder = stringBuilder
            .AppendLine($"private partial class {_transientScopeResolution.Name} : {_containerResolution.TransientScopeInterface.Name}{disposableImplementation}")
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