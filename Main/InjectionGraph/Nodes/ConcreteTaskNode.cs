using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;


internal sealed record ConcreteTaskNodeData(INamedTypeSymbol TaskType)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(TaskType, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public bool Equals(ConcreteTaskNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(TaskType, other.TaskType))
            return false;
        return true;
    }
}

internal sealed class ConcreteTaskNodeManager(Func<ConcreteTaskNodeData, ConcreteTaskNode> factory)
    : ConcreteNodeManagerBase<ConcreteTaskNodeData, ConcreteTaskNode>(factory), IContainerInstance;

internal sealed class ConcreteTaskNode : ConcreteNodeBase
{
    internal ConcreteTaskNode(
        // parameters
        ConcreteTaskNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        Data = data;
        if (data.TaskType is not { TypeArguments: [var innerType] })
            throw new InvalidOperationException("KeyValuePair type must have exactly two type arguments.");
        InnerType = innerType;
        InnerEdge = typeEdgeFactory(this, typeNodeManager.GetOrAddNode(innerType));
    }

    public ITypeSymbol InnerType { get; }

    public TypeEdge InnerEdge { get; }
    
    internal ConcreteTaskNodeData Data { get; }
    
    public override int GetHashCode() => 
        Data.GetHashCode();
    public override bool Equals(object? obj) =>
        obj is ConcreteTaskNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context) => 
        InnerEdge.AddContext(context) 
            ? [(InnerEdge.Target, Location.None)]
            : [];
}