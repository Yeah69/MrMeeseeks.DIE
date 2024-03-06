using MrMeeseeks.DIE.Contexts;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IDisposalHandlingNode
{
    string DisposedFieldReference { get; }
    string DisposedLocalReference { get; }
    string DisposedPropertyReference { get; }
    string DisposableLocalReference { get; }
    string AggregateExceptionReference { get; }
    string AggregateExceptionItemReference { get; }
    string SyncCollectionReference { get; }
    string? AsyncCollectionReference { get; }
    bool HasSyncDisposables { get; }
    bool HasAsyncDisposables { get; }
    string RegisterSyncDisposal();
    string? RegisterAsyncDisposal();
}

internal sealed class DisposalHandlingNode : IDisposalHandlingNode
{
    private bool _syncCollectionUsed;
    private bool _asyncCollectionUsed;
    private readonly string _syncCollection;
    private readonly string? _asyncCollection;
    
    public DisposalHandlingNode(
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext)
    {
        var wellKnownTypes = containerWideContext.WellKnownTypes;
        DisposedFieldReference = referenceGenerator.Generate("_disposed");
        DisposedLocalReference = referenceGenerator.Generate("disposed");
        DisposedPropertyReference = referenceGenerator.Generate("Disposed");
        DisposableLocalReference = referenceGenerator.Generate(wellKnownTypes.IDisposable);
        AggregateExceptionReference = referenceGenerator.Generate(wellKnownTypes.AggregateException);
        AggregateExceptionItemReference = referenceGenerator.Generate("exceptionToAggregate");
        _syncCollection = referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfSyncDisposable);
        _asyncCollection = wellKnownTypes.ConcurrentBagOfAsyncDisposable is not null 
            ? referenceGenerator.Generate(wellKnownTypes.ConcurrentBagOfAsyncDisposable) 
            : null;
    }

    public string DisposedFieldReference { get; }
    public string DisposedLocalReference { get; }
    public string DisposedPropertyReference { get; }
    public string DisposableLocalReference { get; }
    public string AggregateExceptionReference { get; }
    public string AggregateExceptionItemReference { get; }
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
}