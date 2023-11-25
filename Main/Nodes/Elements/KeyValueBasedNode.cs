using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal enum KeyValueBasedType
{
    // ReSharper disable InconsistentNaming
    SingleIEnumerable,
    SingleIAsyncEnumerable,
    SingleIDictionary,
    SingleIReadOnlyDictionary,
    SingleDictionary,
    SingleReadOnlyDictionary,
    SingleSortedDictionary,
    SingleSortedList,
    SingleImmutableDictionary,
    SingleImmutableSortedDictionary
    // ReSharper restore InconsistentNaming
}

internal interface IMapData
{
    string MapTypeFullName { get; }
    string MapReference { get; }
}

internal record SimpleMapData(
        string MapTypeFullName, 
        string MapReference) 
    : IMapData;

internal record ImmutableMapData(
        string MapTypeFullName, 
        string MapReference,
        string ImmutableUngenericTypeFullName) 
    : IMapData;

internal interface IKeyValueBasedNode : IElementNode
{
    KeyValueBasedType Type { get; }
    IMapData? MapData { get; }
    IFunctionCallNode EnumerableCall { get; }
}

internal partial class KeyValueBasedBasedNode(INamedTypeSymbol mapType,
        ITransientScopeWideContext transientScopeWideContext,
        IFunctionNode parentFunction,
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext,
        ICheckIterableTypes checkIterableTypes)
    : IKeyValueBasedNode
{
    private readonly IRangeNode _parentRange = transientScopeWideContext.Range;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections = containerWideContext.WellKnownTypesCollections;

    // parameters
    // dependencies

    public void Build(PassedContext passedContext)
    {
        var isIEnumerable = CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1);
        var isIAsyncEnumerable = CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.IAsyncEnumerable1);
        
        var keyType = isIEnumerable || isIAsyncEnumerable
            ? ((INamedTypeSymbol) mapType.TypeArguments[0]).TypeArguments[0]
            : mapType.TypeArguments[0];
        
        var valueType = isIEnumerable || isIAsyncEnumerable
            ? ((INamedTypeSymbol) mapType.TypeArguments[0]).TypeArguments[1]
            : mapType.TypeArguments[1];
        
        var keyValuePairType = _wellKnownTypesCollections.KeyValuePair2.Construct(keyType, valueType);
        
        var isMulti = checkIterableTypes.MapTypeHasPluralItemType(mapType);

        if (isIEnumerable)
            Type = KeyValueBasedType.SingleIEnumerable;
        if (isIAsyncEnumerable)
            Type = KeyValueBasedType.SingleIAsyncEnumerable;
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.IDictionary2))
        {
            var mapType = _wellKnownTypesCollections.IDictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleIDictionary;
            MapData = new SimpleMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyDictionary2))
        {
            var mapType = _wellKnownTypesCollections.IReadOnlyDictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleIReadOnlyDictionary;
            MapData = new SimpleMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.Dictionary2))
        {
            var mapType = _wellKnownTypesCollections.Dictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleDictionary;
            MapData = new SimpleMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.ReadOnlyDictionary2))
        {
            var mapType = _wellKnownTypesCollections.ReadOnlyDictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleReadOnlyDictionary;
            MapData = new SimpleMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.SortedDictionary2))
        {
            var mapType = _wellKnownTypesCollections.SortedDictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleSortedDictionary;
            MapData = new SimpleMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.SortedList2))
        {
            var mapType = _wellKnownTypesCollections.SortedList2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleSortedList;
            MapData = new SimpleMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType));
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.ImmutableDictionary2))
        {
            var mapType = _wellKnownTypesCollections.ImmutableDictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleImmutableDictionary;
            MapData = new ImmutableMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType),
                _wellKnownTypesCollections.ImmutableDictionary.FullName());
        }
        if (CustomSymbolEqualityComparer.Default.Equals(mapType.OriginalDefinition, _wellKnownTypesCollections.ImmutableSortedDictionary2))
        {
            var mapType = _wellKnownTypesCollections.ImmutableSortedDictionary2.Construct(keyType, valueType);
            Type = KeyValueBasedType.SingleImmutableSortedDictionary;
            MapData = new ImmutableMapData(
                mapType.FullName(), 
                referenceGenerator.Generate(mapType),
                _wellKnownTypesCollections.ImmutableSortedDictionary.FullName());
        }
        
        var enumerableType = Type == KeyValueBasedType.SingleIAsyncEnumerable 
            ? _wellKnownTypesCollections.IAsyncEnumerable1.Construct(keyValuePairType)
            : _wellKnownTypesCollections.IEnumerable1.Construct(keyValuePairType);
        EnumerableCall = isMulti 
            ? _parentRange.BuildEnumerableKeyValueMultiCall(enumerableType, parentFunction)
            : _parentRange.BuildEnumerableKeyValueCall(enumerableType, parentFunction);
    }

    public string TypeFullName => Type != KeyValueBasedType.SingleIEnumerable && Type != KeyValueBasedType.SingleIAsyncEnumerable && MapData is not null
        ? MapData.MapTypeFullName
        : EnumerableCall.TypeFullName;
    public string Reference => Type != KeyValueBasedType.SingleIEnumerable && Type != KeyValueBasedType.SingleIAsyncEnumerable && MapData is not null
        ? MapData.MapReference
        : EnumerableCall.Reference;
    public KeyValueBasedType Type { get; private set; }
    public IMapData? MapData { get; private set; }
    public IFunctionCallNode EnumerableCall { get; private set; } = null!;
}