using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal sealed class TypeNodeManager : IScopeInstance
{
    private readonly Dictionary<ITypeSymbol, TypeNode> _nodes = new(CustomSymbolEqualityComparer.IncludeNullability);
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
    internal void RemoveNode(TypeNode node) =>
        _nodes.Remove(node.Type);
}

internal sealed class TypeNode(ITypeSymbol type) : INode
{
    private readonly List<TypeEdge> _incoming = [];
    private readonly List<ConcreteEdge> _outgoing = [];
    private readonly Dictionary<ScopeNodeContext, HashSet<ScopeNodeContext>> _scopeRootConfiguration = [];
    private readonly Dictionary<ScopeLevel, HashSet<ScopeNodeContext>> _scopeInstanceConfiguration = [];
    private readonly ConcurrentDictionary<int, HashSet<int>> _linkedResolutionIdsToFrom = []; 
    
    internal ITypeSymbol Type { get; } = type;
    internal IReadOnlyList<TypeEdge> Incoming => _incoming;
    internal IReadOnlyList<ConcreteEdge> Outgoing => _outgoing;
    /// <summary>
    /// Use scope root context (key) on all current contexts (value collection).
    /// </summary>
    internal IReadOnlyDictionary<ScopeNodeContext, HashSet<ScopeNodeContext>> ScopeRootConfiguration => _scopeRootConfiguration;
    /// <summary>
    /// Use scope instance level (key; None means "not a scope instance") on all current contexts (value collection).
    /// </summary>
    internal IReadOnlyDictionary<ScopeLevel, HashSet<ScopeNodeContext>> ScopeInstanceConfiguration => _scopeInstanceConfiguration;
    
    internal IReadOnlyDictionary<int, HashSet<int>> LinkedResolutionIdsToFrom => _linkedResolutionIdsToFrom;
    
    internal void AddIncoming(TypeEdge edge) => _incoming.Add(edge);
    internal void AddOutgoing(ConcreteEdge edge) => _outgoing.Add(edge);
    public void RemoveEdge(IEdge edge)
    {
        if (edge is TypeEdge typeEdge)
            _incoming.Remove(typeEdge);
        else if (edge is ConcreteEdge concreteEdge)
            _outgoing.Remove(concreteEdge);
    }
    internal EdgeContext? ContainsOutgoingEdgeFor(EdgeContext context) => 
        _outgoing.SelectMany(edge => edge.Contexts).FirstOrDefault(existing => existing.Equals(context));
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

    internal void RegisterScopeRootConfiguration(ScopeNodeContext scopeRootContext, ScopeNodeContext currentScopeNodeContext)
    {
        if (!_scopeRootConfiguration.TryGetValue(scopeRootContext, out var configuration))
        {
            configuration = [];
            _scopeRootConfiguration[scopeRootContext] = configuration;
        }
        configuration.Add(currentScopeNodeContext);
    }

    internal void RegisterScopeInstanceConfiguration(ScopeLevel scopeLevel, ScopeNodeContext scopeNodeContext)
    {
        if (!_scopeInstanceConfiguration.TryGetValue(scopeLevel, out var configuration))
        {
            configuration = [];
            _scopeInstanceConfiguration[scopeLevel] = configuration;
        }
        configuration.Add(scopeNodeContext);
    }

    internal void LinkResolutionIds(int from, int to) => 
        _linkedResolutionIdsToFrom.GetOrAdd(to, _ => []).Add(from);

    public IReadOnlyList<IEdge> IncomingEdges => _incoming;
}