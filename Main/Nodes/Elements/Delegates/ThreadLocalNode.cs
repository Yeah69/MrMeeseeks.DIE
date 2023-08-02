using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IThreadLocalNode : IDelegateBaseNode
{
    string? SyncDisposalCollectionReference { get; }
}

internal partial class ThreadLocalNode : DelegateBaseNode, IThreadLocalNode
{
    private readonly INamedTypeSymbol _threadLocalType;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IRangeNode _parentRange;

    internal ThreadLocalNode(
        INamedTypeSymbol threadLocalType,
        ILocalFunctionNode function,
        
        ILocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        ITransientScopeWideContext transientScopeWideContext,
        IReferenceGenerator referenceGenerator) 
        : base(threadLocalType, function, localDiagLogger, parentContainer, referenceGenerator)
    {
        _threadLocalType = threadLocalType;
        _parentRange = transientScopeWideContext.Range;
        _checkTypeProperties = transientScopeWideContext.CheckTypeProperties;
    }

    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        base.Build(implementationStack);
        var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(_threadLocalType);
        if (disposalType.HasFlag(DisposalType.Sync))
            SyncDisposalCollectionReference = _parentRange.DisposalHandling.RegisterSyncDisposal();
    }

    public string? SyncDisposalCollectionReference { get; private set; }
}