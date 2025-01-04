using System.Diagnostics.CodeAnalysis;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal class TypeNodeManager : IContainerInstance
{
    private readonly Dictionary<ITypeSymbol, TypeNode> _nodes = [];
    private readonly Func<ITypeSymbol,TypeNode> _factory;

    internal TypeNodeManager(Func<ITypeSymbol, TypeNode> factory) => _factory = factory;
    
    internal IReadOnlyCollection<TypeNode> AllTypeNodes => _nodes.Values;
    
    internal TypeNode GetOrAddNode(ITypeSymbol type)
    {
        if (_nodes.TryGetValue(type, out var node))
            return node;
        node = _factory(type);
        _nodes[type] = node;
        return node;
    }
    
    internal bool TryGetNode(ITypeSymbol type, [NotNullWhen(true)] out TypeNode? node) => _nodes.TryGetValue(type, out node);
}

internal class TypeNode(ITypeSymbol type)
{
    private readonly List<TypeEdge> _incoming = [];
    private readonly List<ConcreteEdge> _outgoing = [];
    
    internal ITypeSymbol Type { get; } = type;
    internal IReadOnlyList<TypeEdge> Incoming => _incoming;
    internal IReadOnlyList<ConcreteEdge> Outgoing => _outgoing;
    
    internal void AddIncoming(TypeEdge edge) => _incoming.Add(edge);
    internal void AddOutgoing(ConcreteEdge edge) => _outgoing.Add(edge);
    internal bool ContainsOutgoingEdgeFor(EdgeContext context) => _outgoing.Any(edge => edge.Contexts.Contains(context));
    internal bool TryGetOutgoingEdgeFor(IConcreteNode concreteNode, [NotNullWhen(true)] out ConcreteEdge? edge)
    {
        foreach (var e in _outgoing.Where(e => Equals(e.Target, concreteNode)))
        {
            edge = e;
            return true;
        }

        edge = null;
        return false;
    }
}