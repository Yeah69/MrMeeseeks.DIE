using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteImplementationNodeData(
    INamedTypeSymbol Implementation,
    IMethodSymbol Constructor,
    IReadOnlyList<IPropertySymbol> ObjectInitializerProperties)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Implementation);
        hash.Add(Constructor);
        foreach (var property in ObjectInitializerProperties)
            hash.Add(property);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteImplementationNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Implementation, other.Implementation))
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Constructor, other.Constructor))
            return false;
        if (ObjectInitializerProperties.Count != other.ObjectInitializerProperties.Count)
            return false;
        for (var i = 0; i < ObjectInitializerProperties.Count; i++)
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(ObjectInitializerProperties[i], other.ObjectInitializerProperties[i]))
                return false;
        return true;
    }
}

internal class ConcreteImplementationNodeManager(Func<ConcreteImplementationNodeData, ConcreteImplementationNode> factory)
    : ConcreteNodeManagerBase<ConcreteImplementationNodeData, ConcreteImplementationNode>(factory), IContainerInstance;

internal class ConcreteImplementationNode : IConcreteNode
{
    internal ConcreteImplementationNode(
        // parameters
        ConcreteImplementationNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        Data = data;
        
        ConstructorParameters = data.Constructor.Parameters
            .Select(p => (
                p.Name,
                typeEdgeFactory(this, typeNodeManager.GetOrAddNode(p.Type)),
                p.Locations.FirstOrDefault() ?? Location.None))
            .ToImmutableArray();
        ObjectInitializerAssignments = data.ObjectInitializerProperties
            .Select(p => (
                p.Name, 
                typeEdgeFactory(this, typeNodeManager.GetOrAddNode(p.Type)),
                p.Locations.FirstOrDefault() ?? Location.None))
            .ToImmutableArray();
    }
    internal ConcreteImplementationNodeData Data { get; }
    internal ImmutableArray<(string Name, TypeEdge Edge, Location Location)> ConstructorParameters { get; }
    internal ImmutableArray<(string Name, TypeEdge Edge, Location Location)> ObjectInitializerAssignments { get; }
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteImplementationNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context)
    {
        var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, Location Location)>();
        foreach (var (_, edge, location) in ConstructorParameters.Concat(ObjectInitializerAssignments))
        {
            if (edge.AddContext(context))
                notYetConnectedTypeNodes.Add((edge.Target, location));
        }
        return notYetConnectedTypeNodes;
    }
}