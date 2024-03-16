using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IThreadLocalNode : IDelegateBaseNode
{
    string? SyncDisposalCollectionReference { get; }
}

internal sealed partial class ThreadLocalNode : DelegateBaseNode, IThreadLocalNode
{
    private readonly INamedTypeSymbol _threadLocalType;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IRangeNode _parentRange;

    internal ThreadLocalNode(
        (INamedTypeSymbol Outer, INamedTypeSymbol Inner) delegateTypes,
        ILocalFunctionNode function,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        ILocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator) 
        : base(delegateTypes, function, typeParameters, localDiagLogger, parentContainer, referenceGenerator)
    {
        _threadLocalType = delegateTypes.Inner;
        _parentRange = parentRange;
        _checkTypeProperties = checkTypeProperties;
    }

    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(_threadLocalType);
        if (disposalType.HasFlag(DisposalType.Sync))
            SyncDisposalCollectionReference = _parentRange.DisposalHandling.RegisterSyncDisposal();
    }

    public string? SyncDisposalCollectionReference { get; private set; }
}