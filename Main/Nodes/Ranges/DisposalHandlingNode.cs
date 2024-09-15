using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IDisposalHandlingNode
{
    string DisposedFieldReference { get; }
    string DisposedLocalReference { get; }
    string DisposedPropertyReference { get; }
    string SyncCollectionReference { get; }
    string? AsyncCollectionReference { get; }
    bool HasSyncDisposables { get; }
    bool HasAsyncDisposables { get; }
    void RegisterSyncDisposal();
    void RegisterAsyncDisposal();
    string CollectionReference { get; }
}

internal sealed class DisposalHandlingNode : IDisposalHandlingNode, ITransientScopeInstance
{
    internal DisposalHandlingNode(
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        DisposedFieldReference = referenceGenerator.Generate("_disposed");
        DisposedLocalReference = referenceGenerator.Generate("disposed");
        DisposedPropertyReference = referenceGenerator.Generate("Disposed");
        SyncCollectionReference = referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfSyncDisposable);
        AsyncCollectionReference = wellKnownTypes.ConcurrentBagOfAsyncDisposable is not null 
            ? referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfAsyncDisposable) 
            : null;
        CollectionReference = referenceGenerator.Generate("_disposal");
    }

    public string DisposedFieldReference { get; }
    public string DisposedLocalReference { get; }
    public string DisposedPropertyReference { get; }
    public string SyncCollectionReference { get; }

    public string? AsyncCollectionReference { get; }

    public bool HasSyncDisposables { get; private set; }

    public bool HasAsyncDisposables { get; private set; }

    public void RegisterSyncDisposal() => HasSyncDisposables = true;

    public void RegisterAsyncDisposal()
    {
        if (AsyncCollectionReference is not null)
            HasAsyncDisposables = true;
    }

    public string CollectionReference { get; }
}