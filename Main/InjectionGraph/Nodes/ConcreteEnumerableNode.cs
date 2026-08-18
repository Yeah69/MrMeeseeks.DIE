using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal abstract record ConcreteEnumerableNodeData(ITypeSymbol EnumerableType, ITypeSymbol MaybeWrappedItemType, bool PurgeKeyAndChoice)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(EnumerableType, CustomSymbolEqualityComparer.IncludeNullability);
        hash.Add(MaybeWrappedItemType, CustomSymbolEqualityComparer.IncludeNullability);
        hash.Add(PurgeKeyAndChoice);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteEnumerableNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(EnumerableType, other.EnumerableType))
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(MaybeWrappedItemType, other.MaybeWrappedItemType))
            return false;
        if (PurgeKeyAndChoice != other.PurgeKeyAndChoice)
            return false;
        return true;
    }

    internal sealed record Interface(ITypeSymbol EnumerableType, ITypeSymbol MaybeWrappedItemType, ImmutableArray<CaseChoiceContext.Single> Choices, bool PurgeKeyAndChoice)
        : ConcreteEnumerableNodeData(EnumerableType, MaybeWrappedItemType, PurgeKeyAndChoice)
    {
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(EnumerableType, CustomSymbolEqualityComparer.IncludeNullability);
            hash.Add(MaybeWrappedItemType, CustomSymbolEqualityComparer.IncludeNullability);
            hash.Add(PurgeKeyAndChoice);
            foreach (var choice in Choices)
                hash.Add(choice);
            return hash.ToHashCode();
        }

        public bool Equals(Interface? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(EnumerableType, other.EnumerableType))
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(MaybeWrappedItemType, other.MaybeWrappedItemType))
                return false;
            if (PurgeKeyAndChoice != other.PurgeKeyAndChoice)
                return false;
            if (!Choices.SequenceEqual(other.Choices))
                return false;
            return true;
        }
    }

    internal sealed record Key(ITypeSymbol EnumerableType, ITypeSymbol MaybeWrappedItemType, ITypeSymbol KeyType, ImmutableArray<object> KeyValues, bool PurgeKeyAndChoice)
        : ConcreteEnumerableNodeData(EnumerableType, MaybeWrappedItemType, PurgeKeyAndChoice)
    {
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(EnumerableType, CustomSymbolEqualityComparer.IncludeNullability);
            hash.Add(MaybeWrappedItemType, CustomSymbolEqualityComparer.IncludeNullability);
            hash.Add(PurgeKeyAndChoice);
            hash.Add(KeyType, CustomSymbolEqualityComparer.IncludeNullability);
            foreach (var keyValue in KeyValues)
                hash.Add(keyValue);
            return hash.ToHashCode();
        }

        public bool Equals(Key? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(EnumerableType, other.EnumerableType))
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(MaybeWrappedItemType, other.MaybeWrappedItemType))
                return false;
            if (PurgeKeyAndChoice != other.PurgeKeyAndChoice)
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(KeyType, other.KeyType))
                return false;
            if (!KeyValues.SequenceEqual(other.KeyValues))
                return false;
            return true;
        }
    }

    internal sealed record SinglePlainItem(ITypeSymbol EnumerableType, ITypeSymbol MaybeWrappedItemType, bool PurgeKeyAndChoice)
        : ConcreteEnumerableNodeData(EnumerableType, MaybeWrappedItemType, PurgeKeyAndChoice)
    {
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(EnumerableType, CustomSymbolEqualityComparer.IncludeNullability);
            hash.Add(MaybeWrappedItemType, CustomSymbolEqualityComparer.IncludeNullability);
            hash.Add(PurgeKeyAndChoice);
            return hash.ToHashCode();
        }

        public bool Equals(SinglePlainItem? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(EnumerableType, other.EnumerableType))
                return false;
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(MaybeWrappedItemType, other.MaybeWrappedItemType))
                return false;
            if (PurgeKeyAndChoice != other.PurgeKeyAndChoice)
                return false;
            return true;
        }
    }
}

internal sealed class ConcreteEnumerableNodeManager(Func<ConcreteEnumerableNodeData, ConcreteEnumerableNode> factory)
    : ConcreteNodeManagerBase<ConcreteEnumerableNodeData, ConcreteEnumerableNode>(factory), IContainerInstance;

internal sealed class ConcreteAsyncEnumerableNodeManager(Func<ConcreteEnumerableNodeData, ConcreteAsyncEnumerableNode> factory)
    : ConcreteNodeManagerBase<ConcreteEnumerableNodeData, ConcreteAsyncEnumerableNode>(factory), IContainerInstance;

internal abstract class ConcreteEnumerableNodeBase : ConcreteNodeBase
{
    private readonly Lazy<TypeEdge> _innerEdgeLazy;

    internal ConcreteEnumerableNodeBase(
        // parameters
        ConcreteEnumerableNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        Data = data;
        _innerEdgeLazy = new(() => typeEdgeFactory(this, typeNodeManager.GetOrAddNode(data.MaybeWrappedItemType)));
    }

    internal TypeEdge InnerEdge => _innerEdgeLazy.Value;

    internal ConcreteEnumerableNodeData Data { get; }
    
    public override int GetHashCode() => 
        Data.GetHashCode();
    public override bool Equals(object? obj) => 
        obj is ConcreteEnumerableNodeBase other && Data.Equals(other.Data);

    public IReadOnlyList<(TypeNode TypeNode, EdgeContext NewContext, Location Location)> ConnectIfNotAlready(EdgeContext context)
    {
        switch (Data)
        {
            // KeyValuePair involved
            case ConcreteEnumerableNodeData.Key { KeyValues: var keyValues, KeyType: var keyType }:
            {
                var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, EdgeContext NewContext, Location Location)>();
                foreach (var value in keyValues)
                {
                    var newContext = context with { Key = new KeyContext.Single(keyType, value) };
                    
                    if (InnerEdge.AddContext(newContext))
                        notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
                }

                return notYetConnectedTypeNodes;
            }
            // Vanilla case: No KeyValuePair
            case ConcreteEnumerableNodeData.Interface { Choices: var caseChoices}:
            {
                var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, EdgeContext NewContext, Location Location)>();
                foreach (var caseChoice in caseChoices)
                {
                    var newContext = context with { CaseChoice = caseChoice };
                
                    if (InnerEdge.AddContext(newContext))
                        notYetConnectedTypeNodes.Add((InnerEdge.Target, newContext, Location.None));
                }

                return notYetConnectedTypeNodes;
            }
            case ConcreteEnumerableNodeData.SinglePlainItem:
            {
                var purgedContext = context with { CaseChoice = new CaseChoiceContext.None2(), Key = new KeyContext.None1() };
        
                if (InnerEdge.AddContext(purgedContext))
                    return [(InnerEdge.Target, purgedContext, Location.None)];
                break;
            }
        }

        return [];
    }
}

internal sealed class ConcreteEnumerableNode(
    // parameters
    ConcreteEnumerableNodeData data,

    // dependencies
    TypeNodeManager typeNodeManager,
    Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    : ConcreteEnumerableNodeBase(data, typeNodeManager, typeEdgeFactory)
{
    public override bool Equals(object? obj) => 
        obj is ConcreteEnumerableNode && base.Equals(obj);
}

internal sealed class ConcreteAsyncEnumerableNode(
    // parameters
    ConcreteEnumerableNodeData data,

    // dependencies
    TypeNodeManager typeNodeManager,
    Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory) 
    : ConcreteEnumerableNodeBase(data, typeNodeManager, typeEdgeFactory)
{
    public override bool Equals(object? obj) => 
        obj is ConcreteAsyncEnumerableNode && base.Equals(obj);
}