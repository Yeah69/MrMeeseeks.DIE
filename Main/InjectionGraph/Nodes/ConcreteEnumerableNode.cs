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

internal record ConcreteEnumerableNodeSequenceData(IReadOnlyList<ITypeSymbol> Sequence)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var type in Sequence)
            hash.Add(type, CustomSymbolEqualityComparer.IncludeNullability);
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
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Sequence[i], other.Sequence[i]))
                return false;
        return true;
    }
}

internal class ConcreteEnumerableNodeManager(Func<ConcreteEnumerableNodeData, ConcreteEnumerableNode> factory)
    : ConcreteNodeManagerBase<ConcreteEnumerableNodeData, ConcreteEnumerableNode>(factory), IContainerInstance;

internal class ConcreteEnumerableNode : IConcreteNode
{
    private readonly IdRegister _idRegister;
    private readonly Dictionary<DomainContext, ImmutableArray<int>> _sequences = [];
    private readonly Lazy<TypeEdge> _innerEdgeLazy;

    internal ConcreteEnumerableNode(
        // parameters
        ConcreteEnumerableNodeData data,

        // dependencies
        IdRegister idRegister,
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory,
        WellKnownTypes wellKnownTypes)
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
        UnwrappedInnerType = TypeSymbolUtility.GetUnwrappedType(maybeWrappedInnerType, wellKnownTypes);
        OutwardFacingTypeId = idRegister.GetOutwardFacingTypeId(UnwrappedInnerType);
        _innerEdgeLazy = new Lazy<TypeEdge>(() => typeEdgeFactory(this, typeNodeManager.GetOrAddNode(maybeWrappedInnerType)));
    }

    internal TypeEdge InnerEdge => _innerEdgeLazy.Value;
    
    internal ITypeSymbol UnwrappedInnerType { get; }

    internal ConcreteEnumerableNodeData Data { get; }
    internal int OutwardFacingTypeId { get; }
    internal IReadOnlyDictionary<DomainContext, ImmutableArray<int>> Sequences => _sequences;
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteInterfaceNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, EdgeContext NewContext, Location Location)> ConnectIfNotAlready(
        EdgeContext context, 
        ConcreteEnumerableNodeSequenceData sequenceData)
    {
        if (!_sequences.TryGetValue(context.Domain, out var sequence))
        {
            sequence = [..sequenceData.Sequence.Select(t => _idRegister.GetInitialCaseId(context.Domain, t))];
            _sequences[context.Domain] = sequence;
        }
        
        var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, EdgeContext NewContext, Location Location)>();
        foreach (var caseId in sequence)
        {
            var newContext = context with { InitialInitialCaseChoice = new InitialCaseChoiceContext.Single(OutwardFacingTypeId, caseId) };
            if (InnerEdge.AddContext(newContext))
                notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
        }
        return notYetConnectedTypeNodes;
    }
}