using System.Collections.Concurrent;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal sealed record ConcreteEnumerableNodeData(ITypeSymbol Enumerable)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Enumerable, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public bool Equals(ConcreteEnumerableNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Enumerable, other.Enumerable))
            return false;
        return true;
    }
}

internal sealed class ConcreteEnumerableNodeManager(Func<ConcreteEnumerableNodeData, ConcreteEnumerableNode> factory)
    : ConcreteNodeManagerBase<ConcreteEnumerableNodeData, ConcreteEnumerableNode>(factory), IContainerInstance;

internal abstract record ConcreteEnumerableResult
{
    internal sealed record Interface(ImmutableArray<CaseChoiceContext.Single> Choices) : ConcreteEnumerableResult;
    internal sealed record Key(ITypeSymbol KeyType, ImmutableArray<object> KeyValues) : ConcreteEnumerableResult;

    internal sealed record SinglePlainItem : ConcreteEnumerableResult;
    
    internal bool PurgeKeyAndChoice { get; set; }
}

internal sealed class ConcreteEnumerableNode : IConcreteNode
{
    private readonly IContainerCheckTypeProperties _containerCheckTypeProperties;
    private readonly IdRegister _idRegister;
    private readonly ConcurrentDictionary<NodeContext, ConcurrentDictionary<KeyContext, ConcreteEnumerableResult>> _collectionCases = [];
    private readonly Lazy<TypeEdge> _innerEdgeLazy;

    internal ConcreteEnumerableNode(
        // parameters
        ConcreteEnumerableNodeData data,

        // dependencies
        ICheckIterableTypes checkIterableTypes,
        IContainerCheckTypeProperties containerCheckTypeProperties,
        IdRegister idRegister,
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory,
        TypeSymbolUtility typeSymbolUtility,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _containerCheckTypeProperties = containerCheckTypeProperties;
        _idRegister = idRegister;
        Data = data;

        var maybeWrappedItemType = data.Enumerable switch
        {
            INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedType => namedType.TypeArguments[0],
            IArrayTypeSymbol arrayType => arrayType.ElementType,
            _ => throw new InvalidOperationException(
                $"The enumerable type '{data.Enumerable}' is not supported. It must be a generic type with one type argument or an array type.")
        };
        var tempUnwrappedItemType = typeSymbolUtility.GetUnwrappedType(maybeWrappedItemType);
        if (CustomSymbolEqualityComparer.Default.Equals(tempUnwrappedItemType.OriginalDefinition,
                wellKnownTypesCollections.KeyValuePair2)
            && tempUnwrappedItemType is INamedTypeSymbol { TypeArguments: [var keyType, var valueType] })
        {
            KeyValuePairKeyType = keyType;
            tempUnwrappedItemType = typeSymbolUtility.GetUnwrappedType(valueType);
            IsKeyValuePairWithCollectionValue = checkIterableTypes.IsCollectionType(tempUnwrappedItemType);
        }

        UnwrappedItemType = tempUnwrappedItemType;
        _innerEdgeLazy = new Lazy<TypeEdge>(() => typeEdgeFactory(this, typeNodeManager.GetOrAddNode(maybeWrappedItemType)));
    }

    internal TypeEdge InnerEdge => _innerEdgeLazy.Value;
    
    internal ITypeSymbol? KeyValuePairKeyType { get; }
    internal bool IsKeyValuePairWithCollectionValue { get; }
    internal ITypeSymbol UnwrappedItemType { get; }

    internal ConcreteEnumerableNodeData Data { get; }
    internal ConcurrentDictionary<NodeContext, ConcurrentDictionary<KeyContext, ConcreteEnumerableResult>> CollectionCases => _collectionCases;
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteEnumerableNode other && Data.Equals(other.Data);

    public IReadOnlyList<(TypeNode TypeNode, EdgeContext NewContext, Location Location)> ConnectIfNotAlready(EdgeContext context)
    {
        // KeyValuePair involved
        if (KeyValuePairKeyType is {} keyValuePairKeyType && UnwrappedItemType is INamedTypeSymbol { TypeKind: TypeKind.Interface } unwrappedItemType)
        {
            var keyValues = (IsKeyValuePairWithCollectionValue 
                ? _containerCheckTypeProperties.MapToKeyedMultipleImplementations(unwrappedItemType, keyValuePairKeyType).Select(kvp => kvp.Key)
                : _containerCheckTypeProperties.MapToKeyedImplementations(unwrappedItemType, keyValuePairKeyType).Select(kvp => kvp.Key))
                .ToImmutableArray();

            var keyResult = _collectionCases.GetOrAdd(context.Node, _ => new ConcurrentDictionary<KeyContext, ConcreteEnumerableResult>())
                .GetOrAdd(new KeyContext.None(), new ConcreteEnumerableResult.Key(keyValuePairKeyType, keyValues));
            keyResult.PurgeKeyAndChoice = context is { Key: not KeyContext.None } or { CaseChoice: not CaseChoiceContext.None };
                
            var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, EdgeContext NewContext, Location Location)>();
            foreach (var value in keyValues)
            {
                var newContext = context with { Key = new KeyContext.Single(keyValuePairKeyType, value) };
                    
                if (InnerEdge.AddContext(newContext))
                    notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
            }

            return notYetConnectedTypeNodes;
        }
        var injectionKey = context.Key is KeyContext.Single { Type: var keyType, Value: var keyValue } 
            ? new InjectionKey(keyType, keyValue)
            : null;
        // Vanilla case: No KeyValuePair
        if (UnwrappedItemType is INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceType)
        {
            var outwardFacingId = _idRegister.GetOutwardFacingTypeId(interfaceType);
            var caseChoices = _containerCheckTypeProperties.MapToImplementations(interfaceType, injectionKey)
                .Select(i => _idRegister.GetInitialCaseId(context.Node, interfaceType, i))
                .OfType<IdRegister.CaseIdResponse.Success>()
                .Select(s => new CaseChoiceContext.Single(outwardFacingId, s.NextCaseId))
                .ToImmutableArray();

            var interfaceResult = _collectionCases.GetOrAdd(context.Node, _ => new ConcurrentDictionary<KeyContext, ConcreteEnumerableResult>())
                .GetOrAdd(context.Key, new ConcreteEnumerableResult.Interface(caseChoices));
            interfaceResult.PurgeKeyAndChoice = context is { Key: not KeyContext.None } or { CaseChoice: not CaseChoiceContext.None };
            
            var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, EdgeContext NewContext, Location Location)>();
            foreach (var caseChoice in caseChoices)
            {
                var newContext = context with { CaseChoice = caseChoice };
                
                if (InnerEdge.AddContext(newContext))
                    notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
            }

            return notYetConnectedTypeNodes;
        }

        var singlePlainItemResult = _collectionCases.GetOrAdd(context.Node, _ => new ConcurrentDictionary<KeyContext, ConcreteEnumerableResult>())
            .GetOrAdd(context.Key, new ConcreteEnumerableResult.SinglePlainItem());
        singlePlainItemResult.PurgeKeyAndChoice = context is { Key: not KeyContext.None } or { CaseChoice: not CaseChoiceContext.None };

        var purgedContext = context with { CaseChoice = new CaseChoiceContext.None(), Key = new KeyContext.None() };
        
        if (InnerEdge.AddContext(purgedContext))
            return [(InnerEdge.Target, purgedContext, Location.None)];

        return [];
        /*
        if (!_sequences.TryGetValue(context.Domain, out var sequence))
        {
            sequence = [..sequenceData.Sequence];
            _sequences[context.Domain] = sequence;
        }
        
        var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, EdgeContext NewContext, Location Location)>();
        foreach (var yield in sequence)
        {
            var newContext = yield switch
            {
                ConcreteEnumerableYield.Case(var outwardFacingTypeId, var caseId) => 
                    context with { InitialCaseChoice = new InitialCaseChoiceContext.Single(outwardFacingTypeId, caseId) },
                ConcreteEnumerableYield.Key(var keyType, var keyObject) =>
                    context with { Key = new KeyContext.Single(keyType, keyObject) },
                _ => throw new InvalidOperationException($"Unknown yield type: {yield.GetType()}")
            };
            if (InnerEdge.AddContext(newContext))
                notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
        }
        return notYetConnectedTypeNodes;*/
    }
}