using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal enum EnumerableBasedType
{
    IEnumerable,
    Array,
    IList,
    ICollection,
    ReadOnlyCollection,
    IReadOnlyCollection,
    IReadOnlyList,
    ArraySegment,
    ConcurrentBag,
    ConcurrentQueue,
    ConcurrentStack,
    HashSet,
    LinkedList,
    List,
    Queue,
    SortedSet,
    Stack,
    ImmutableArray,
    ImmutableHashSet,
    ImmutableList,
    ImmutableQueue,
    ImmutableSortedSet,
    ImmutableStack,
    
    IAsyncEnumerable
}

internal interface ICollectionData
{
    string CollectionTypeFullName { get; }
    string CollectionReference { get; }
}

internal record SimpleCollectionData(
    string CollectionTypeFullName, 
    string CollectionReference) 
    : ICollectionData;

internal record ImmutableCollectionData(
        string CollectionTypeFullName, 
        string CollectionReference,
        string ImmutableUngenericTypeFullName) 
    : ICollectionData;

internal record ReadOnlyCollectionData(
        string CollectionTypeFullName, 
        string CollectionReference,
        string ConcreteReadOnlyCollectionTypeFullName) 
    : ICollectionData;

internal interface IEnumerableBasedNode : IElementNode, IPotentiallyAwaitedNode, IOnAwait
{
    EnumerableBasedType Type { get; }
    ICollectionData? CollectionData { get; }
    IFunctionCallNode EnumerableCall { get; }
}

internal class EnumerableBasedNode : IEnumerableBasedNode
{
    private readonly ITypeSymbol _collectionType;
    private readonly IRangeNode _parentRange;
    private readonly IFunctionNode _parentFunction;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    public EnumerableBasedNode(
        ITypeSymbol collectionType,
        IRangeNode parentRange,
        IFunctionNode parentFunction,
        IReferenceGenerator referenceGenerator,
        
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _collectionType = collectionType;
        _parentRange = parentRange;
        _parentFunction = parentFunction;
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
        AsyncReference = referenceGenerator.Generate("result");
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        var collectionsInnerType = CollectionUtility.GetCollectionsInnerType(_collectionType);
        
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1))
            Type = EnumerableBasedType.IEnumerable;
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.IAsyncEnumerable1))
            Type = EnumerableBasedType.IAsyncEnumerable;
        if (_collectionType is IArrayTypeSymbol)
        {
            Type = EnumerableBasedType.Array;
            CollectionData = new SimpleCollectionData(
                $"{collectionsInnerType}[]", 
                _referenceGenerator.Generate("array"));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.IList1))
        {
            var collectionType = _wellKnownTypesCollections.IList1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.IList;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ICollection1))
        {
            var collectionType = _wellKnownTypesCollections.ICollection1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ICollection;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ReadOnlyCollection1))
        {
            var collectionType = _wellKnownTypesCollections.ReadOnlyCollection1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ReadOnlyCollection;
            CollectionData = new ReadOnlyCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ReadOnlyCollection1.Construct(collectionsInnerType).FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyCollection1))
        {
            var collectionType = _wellKnownTypesCollections.IReadOnlyCollection1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.IReadOnlyCollection;
            CollectionData = new ReadOnlyCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ReadOnlyCollection1.Construct(collectionsInnerType).FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyList1))
        {
            var collectionType = _wellKnownTypesCollections.IReadOnlyList1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.IReadOnlyList;
            CollectionData = new ReadOnlyCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ReadOnlyCollection1.Construct(collectionsInnerType).FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ArraySegment1))
        {
            var collectionType = _wellKnownTypesCollections.ArraySegment1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ArraySegment;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ConcurrentBag1))
        {
            var collectionType = _wellKnownTypesCollections.ConcurrentBag1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ConcurrentBag;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ConcurrentQueue1))
        {
            var collectionType = _wellKnownTypesCollections.ConcurrentQueue1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ConcurrentQueue;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ConcurrentStack1))
        {
            var collectionType = _wellKnownTypesCollections.ConcurrentStack1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ConcurrentStack;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.HashSet1))
        {
            var collectionType = _wellKnownTypesCollections.HashSet1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.HashSet;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.LinkedList1))
        {
            var collectionType = _wellKnownTypesCollections.LinkedList1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.LinkedList;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.List1))
        {
            var collectionType = _wellKnownTypesCollections.List1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.List;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.Queue1))
        {
            var collectionType = _wellKnownTypesCollections.Queue1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.Queue;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.SortedSet1))
        {
            var collectionType = _wellKnownTypesCollections.SortedSet1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.SortedSet;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.Stack1))
        {
            var collectionType = _wellKnownTypesCollections.Stack1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.Stack;
            CollectionData = new SimpleCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ImmutableArray1))
        {
            var collectionType = _wellKnownTypesCollections.ImmutableArray1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ImmutableArray;
            CollectionData = new ImmutableCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ImmutableArray.FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ImmutableHashSet1))
        {
            var collectionType = _wellKnownTypesCollections.ImmutableHashSet1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ImmutableHashSet;
            CollectionData = new ImmutableCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ImmutableHashSet.FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ImmutableList1))
        {
            var collectionType = _wellKnownTypesCollections.ImmutableList1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ImmutableList;
            CollectionData = new ImmutableCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ImmutableList.FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ImmutableQueue1))
        {
            var collectionType = _wellKnownTypesCollections.ImmutableQueue1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ImmutableQueue;
            CollectionData = new ImmutableCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ImmutableQueue.FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ImmutableSortedSet1))
        {
            var collectionType = _wellKnownTypesCollections.ImmutableSortedSet1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ImmutableSortedSet;
            CollectionData = new ImmutableCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ImmutableSortedSet.FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(_collectionType.OriginalDefinition, _wellKnownTypesCollections.ImmutableStack1))
        {
            var collectionType = _wellKnownTypesCollections.ImmutableStack1.Construct(collectionsInnerType);
            Type = EnumerableBasedType.ImmutableStack;
            CollectionData = new ImmutableCollectionData(
                collectionType.FullName(), 
                _referenceGenerator.Generate(collectionType),
                _wellKnownTypesCollections.ImmutableStack.FullName());
        }
        
        var enumerableType = Type == EnumerableBasedType.IAsyncEnumerable 
            ? _wellKnownTypesCollections.IAsyncEnumerable1.Construct(collectionsInnerType)
            : _wellKnownTypesCollections.IEnumerable1.Construct(collectionsInnerType);
        EnumerableCall = _parentRange.BuildEnumerableCall(enumerableType, _parentFunction, this);
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitEnumerableBasedNode(this);

    public string TypeFullName => Type != EnumerableBasedType.IEnumerable && Type != EnumerableBasedType.IAsyncEnumerable && CollectionData is not null
        ? CollectionData.CollectionTypeFullName
        : EnumerableCall.TypeFullName;
    public string Reference => Type != EnumerableBasedType.IEnumerable && Type != EnumerableBasedType.IAsyncEnumerable && CollectionData is not null
        ? CollectionData.CollectionReference
        : EnumerableCall.Reference;
    public EnumerableBasedType Type { get; private set; }
    public ICollectionData? CollectionData { get; private set; }
    public IFunctionCallNode EnumerableCall { get; private set; } = null!;

    public bool Awaited
    {
        get => EnumerableCall.Awaited; 
        set => EnumerableCall.Awaited = value;
    }
    public string? AsyncReference { get; }

    public string? AsyncTypeFullName => SynchronicityDecision switch
    {
        SynchronicityDecision.AsyncTask => _wellKnownTypes.Task1.Construct(_collectionType).FullName(),
        SynchronicityDecision.AsyncValueTask => _wellKnownTypes.ValueTask1.Construct(_collectionType).FullName(),
        _ => _collectionType.FullName()
    };
    public SynchronicityDecision SynchronicityDecision => EnumerableCall.SynchronicityDecision;
    public void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode) => _parentFunction.OnAwait(this);
}