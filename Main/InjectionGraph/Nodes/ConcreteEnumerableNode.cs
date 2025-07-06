using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteEnumerableNodeData(ITypeSymbol Enumerable)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Enumerable, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteEnumerableNodeData? other)
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

internal record ConcreteEnumerableNodeSequenceData(IReadOnlyList<ConcreteEnumerableYield> Sequence)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var yield in Sequence)
            hash.Add(yield);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteEnumerableNodeSequenceData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (Sequence.Count != other.Sequence.Count)
            return false;
        for (var i = 0; i < Sequence.Count; i++)
            if (!Sequence[i].Equals(other.Sequence[i]))
                return false;
        return true;
    }
}

internal class ConcreteEnumerableNodeManager(Func<ConcreteEnumerableNodeData, ConcreteEnumerableNode> factory)
    : ConcreteNodeManagerBase<ConcreteEnumerableNodeData, ConcreteEnumerableNode>(factory), IContainerInstance;

internal abstract record ConcreteEnumerableYield
{
    internal sealed record Case(int OutwardFacingTypeId, int CaseId) : ConcreteEnumerableYield;
    internal sealed record Key(ITypeSymbol KeyType, object KeyObject) : ConcreteEnumerableYield;
}

internal class ConcreteEnumerableNode : IConcreteNode
{
    private readonly IdRegister _idRegister;
    private readonly Dictionary<DomainContext, ImmutableArray<ConcreteEnumerableYield>> _sequences = [];
    private readonly Lazy<TypeEdge> _innerEdgeLazy;

    internal ConcreteEnumerableNode(
        // parameters
        ConcreteEnumerableNodeData data,

        // dependencies
        ICheckIterableTypes checkIterableTypes,
        IdRegister idRegister,
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
    {
        _idRegister = idRegister;
        Data = data;

        var maybeWrappedInnerType = data.Enumerable switch
        {
            INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedType => namedType.TypeArguments[0],
            IArrayTypeSymbol arrayType => arrayType.ElementType,
            _ => throw new InvalidOperationException(
                $"The enumerable type '{data.Enumerable}' is not supported. It must be a generic type with one type argument or an array type.")
        };
        var tempUnwrappedInnerType = TypeSymbolUtility.GetUnwrappedType(maybeWrappedInnerType, wellKnownTypes);
        if (CustomSymbolEqualityComparer.Default.Equals(tempUnwrappedInnerType.OriginalDefinition,
                wellKnownTypesCollections.KeyValuePair2)
            && tempUnwrappedInnerType is INamedTypeSymbol { TypeArguments: [var keyType, var valueType] })
        {
            KeyType = keyType;
            tempUnwrappedInnerType = TypeSymbolUtility.GetUnwrappedType(valueType, wellKnownTypes);
            IsKeyedMultiple = checkIterableTypes.IsCollectionType(tempUnwrappedInnerType);
        }

        UnwrappedInnerType = tempUnwrappedInnerType;
        _innerEdgeLazy = new Lazy<TypeEdge>(() => typeEdgeFactory(this, typeNodeManager.GetOrAddNode(maybeWrappedInnerType)));
    }

    internal TypeEdge InnerEdge => _innerEdgeLazy.Value;
    
    internal ITypeSymbol? KeyType { get; }
    internal bool IsKeyedMultiple { get; }
    internal ITypeSymbol UnwrappedInnerType { get; }

    internal ConcreteEnumerableNodeData Data { get; }
    internal IReadOnlyDictionary<DomainContext, ImmutableArray<ConcreteEnumerableYield>> Sequences => _sequences;
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteEnumerableNode other && Data.Equals(other.Data);

    public IReadOnlyList<(TypeNode TypeNode, EdgeContext NewContext, Location Location)> ConnectIfNotAlready(
        EdgeContext context, 
        ConcreteEnumerableNodeSequenceData sequenceData)
    {
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
                    context with { InitialInitialCaseChoice = new InitialCaseChoiceContext.Single(outwardFacingTypeId, caseId) },
                ConcreteEnumerableYield.Key(var keyType, var keyObject) =>
                    context with { Key = new KeyContext.Single(keyType, keyObject) },
                _ => throw new InvalidOperationException($"Unknown yield type: {yield.GetType()}")
            };
            if (InnerEdge.AddContext(newContext))
                notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
        }
        return notYetConnectedTypeNodes;
    }
}