using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IThreadLocalNode : IDelegateBaseNode
{
    string? SubDisposalReference { get; }
}

internal sealed partial class ThreadLocalNode : DelegateBaseNode, IThreadLocalNode
{
    private readonly INamedTypeSymbol _threadLocalType;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IRangeNode _parentRange;
    private readonly IFunctionNode _parentFunction;

    internal ThreadLocalNode(
        (INamedTypeSymbol Outer, INamedTypeSymbol Inner) delegateTypes,
        ILocalFunctionNode function,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        ILocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        IFunctionNode parentFunction,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator) 
        : base(delegateTypes, function, typeParameters, localDiagLogger, parentContainer, referenceGenerator)
    {
        _threadLocalType = delegateTypes.Inner;
        _parentRange = parentRange;
        _parentFunction = parentFunction;
        _checkTypeProperties = checkTypeProperties;
    }

    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(_threadLocalType);
        if (disposalType is not DisposalType.None && disposalType.HasFlag(DisposalType.Sync)) 
            _parentRange.RegisterTypeForDisposal(_threadLocalType);
        if (disposalType.HasFlag(DisposalType.Sync))
            SubDisposalReference = _parentFunction.SubDisposalNode.Reference;
    }

    public string? SubDisposalReference { get; private set; }
}