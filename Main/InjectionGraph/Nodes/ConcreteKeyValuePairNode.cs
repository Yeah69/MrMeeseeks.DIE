using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;


internal record ConcreteKeyValuePairNodeData(INamedTypeSymbol KeyValuePairType)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(KeyValuePairType, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteKeyValuePairNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(KeyValuePairType, other.KeyValuePairType))
            return false;
        return true;
    }
}

internal class ConcreteKeyValuePairNodeManager(Func<ConcreteKeyValuePairNodeData, ConcreteKeyValuePairNode> factory)
    : ConcreteNodeManagerBase<ConcreteKeyValuePairNodeData, ConcreteKeyValuePairNode>(factory), IContainerInstance;

internal class ConcreteKeyValuePairNode : IConcreteNode
{
    internal ConcreteKeyValuePairNode(
        // parameters
        ConcreteKeyValuePairNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        Data = data;
        if (data.KeyValuePairType is not { TypeArguments: [var keyType, var valueType] })
            throw new InvalidOperationException("KeyValuePair type must have exactly two type arguments.");
        KeyType = keyType;
        ValueEdge = typeEdgeFactory(this, typeNodeManager.GetOrAddNode(valueType));
    }

    public ITypeSymbol KeyType { get; set; }

    public TypeEdge ValueEdge { get; }
    
    internal ConcreteKeyValuePairNodeData Data { get; }
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteKeyValuePairNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context) => 
        ValueEdge.AddContext(context) 
            ? [(ValueEdge.Target, Location.None)]
            : [];
}