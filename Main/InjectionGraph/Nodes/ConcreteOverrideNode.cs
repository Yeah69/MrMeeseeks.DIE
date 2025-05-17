using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteOverrideNodeData(ITypeSymbol Type)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteOverrideNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Type, other.Type))
            return false;
        return true;
    }
}

internal class ConcreteOverrideNodeManager(Func<ConcreteOverrideNodeData, ConcreteOverrideNode> factory)
    : ConcreteNodeManagerBase<ConcreteOverrideNodeData, ConcreteOverrideNode>(factory), IContainerInstance;

internal class ConcreteOverrideNode : IConcreteNode
{
    internal ConcreteOverrideNode(
        // parameters
        ConcreteOverrideNodeData data) =>
        Data = data;

    internal ConcreteOverrideNodeData Data { get; }
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteOverrideNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context) => [];
}