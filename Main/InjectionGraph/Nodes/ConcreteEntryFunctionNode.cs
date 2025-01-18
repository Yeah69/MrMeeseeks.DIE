using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteEntryFunctionNodeData(string Name, ITypeSymbol ReturnType, IReadOnlyList<ITypeSymbol> ParameterTypes)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(ReturnType);
        foreach (var parameterType in ParameterTypes)
            hash.Add(parameterType);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteEntryFunctionNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (Name != other.Name)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(ReturnType, other.ReturnType))
            return false;
        if (ParameterTypes.Count != other.ParameterTypes.Count)
            return false;
        for (var i = 0; i < ParameterTypes.Count; i++)
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(ParameterTypes[i], other.ParameterTypes[i]))
                return false;
        return true;
    }
}

internal class ConcreteEntryFunctionNodeManager(Func<ConcreteEntryFunctionNodeData, ConcreteEntryFunctionNode> factory)
    : ConcreteNodeManagerBase<ConcreteEntryFunctionNodeData, ConcreteEntryFunctionNode>(factory), IContainerInstance;

internal class ConcreteEntryFunctionNode : IConcreteNode
{
    internal ConcreteEntryFunctionNode(
        // parameters
        ConcreteEntryFunctionNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        Data = data;
        ReturnType = typeEdgeFactory(this, typeNodeManager.GetOrAddNode(data.ReturnType));
    }

    public TypeEdge ReturnType { get; }

    public ConcreteEntryFunctionNodeData Data { get; }
    
    public override int GetHashCode() => Data.GetHashCode();

    public override bool Equals(object? obj) => obj is ConcreteEntryFunctionNode node && Data.Equals(node.Data);
    
    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context) => 
        ReturnType.AddContext(context) 
            ? [(ReturnType.Target, Location.None)] 
            : Array.Empty<(TypeNode TypeNode, Location Location)>();
}