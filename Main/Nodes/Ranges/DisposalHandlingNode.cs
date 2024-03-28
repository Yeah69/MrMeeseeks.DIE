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
    string RegisterSyncDisposal();
    string? RegisterAsyncDisposal();
    string CollectionReference { get; }
}

internal sealed class DisposalHandlingNode : IDisposalHandlingNode
{
    private bool _syncCollectionUsed;
    private bool _asyncCollectionUsed;
    private readonly string _syncCollection;
    private readonly string? _asyncCollection;
    
    internal DisposalHandlingNode(
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        DisposedFieldReference = referenceGenerator.Generate("_disposed");
        DisposedLocalReference = referenceGenerator.Generate("disposed");
        DisposedPropertyReference = referenceGenerator.Generate("Disposed");
        _syncCollection = referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfSyncDisposable);
        _asyncCollection = wellKnownTypes.ConcurrentBagOfAsyncDisposable is not null 
            ? referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfAsyncDisposable) 
            : null;
        CollectionReference = referenceGenerator.Generate("_disposal");
    }

    public string DisposedFieldReference { get; }
    public string DisposedLocalReference { get; }
    public string DisposedPropertyReference { get; }
    public string SyncCollectionReference => _syncCollection;
    public string? AsyncCollectionReference => _asyncCollection;
    public bool HasSyncDisposables => _syncCollectionUsed;
    public bool HasAsyncDisposables => _asyncCollectionUsed;

    public string RegisterSyncDisposal()
    {
        _syncCollectionUsed = true;
        return _syncCollection;
    }

    public string? RegisterAsyncDisposal()
    {
        if (_asyncCollection is not null)
            _asyncCollectionUsed = true;
        
        return _asyncCollection;
    }

    public string CollectionReference { get; }
}