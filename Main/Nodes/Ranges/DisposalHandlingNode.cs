namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IDisposalHandlingNode
{
    string DisposedFieldReference { get; }
    string DisposedLocalReference { get; }
    string DisposedPropertyReference { get; }
    string DisposableLocalReference { get; }
    string? SyncCollectionReference { get; }
    string? AsyncCollectionReference { get; }
    bool HasSyncDisposables { get; }
    bool HasAsyncDisposables { get; }
    string RegisterSyncDisposal();
    string RegisterAsyncDisposal();
}

internal class DisposalHandlingNode : IDisposalHandlingNode
{
    private bool _syncCollectionUsed;
    private bool _asyncCollectionUsed;
    private readonly string _syncCollection;
    private readonly string _asyncCollection;
    
    public DisposalHandlingNode(
        IReferenceGenerator referenceGenerator,
        
        WellKnownTypes wellKnownTypes)
    {
        DisposedFieldReference = referenceGenerator.Generate("_disposed");
        DisposedLocalReference = referenceGenerator.Generate("disposed");
        DisposedPropertyReference = referenceGenerator.Generate("Disposed");
        DisposableLocalReference = referenceGenerator.Generate("disposable");
        _syncCollection = referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfSyncDisposable);
        _asyncCollection = referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfAsyncDisposable);
    }

    public string DisposedFieldReference { get; }
    public string DisposedLocalReference { get; }
    public string DisposedPropertyReference { get; }
    public string DisposableLocalReference { get; }
    public string? SyncCollectionReference => _syncCollectionUsed ? _syncCollection : null;
    public string? AsyncCollectionReference => _asyncCollectionUsed ? _asyncCollection : null;
    public bool HasSyncDisposables => _syncCollectionUsed;
    public bool HasAsyncDisposables => _asyncCollectionUsed;

    public string RegisterSyncDisposal()
    {
        _syncCollectionUsed = true;
        return _syncCollection;
    }

    public string RegisterAsyncDisposal()
    {
        _asyncCollectionUsed = true;
        return _asyncCollection;
    }
}