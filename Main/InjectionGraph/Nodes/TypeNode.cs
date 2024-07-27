using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal sealed class TypeNodeManager : IContainerInstance
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
}

internal sealed class TypeNode(ITypeSymbol type) : INode
{
    private readonly List<TypeEdge> _incomingTypeEdges = [];
    private readonly List<TypeTypeEdge> _incomingTypeTypeEdges = [];
    private readonly List<ConcreteEdge> _outgoingConcreteEdges = [];
    private readonly Dictionary<ScopeNodeContext, (HashSet<ScopeNodeContext> PreviousScopeNodeContexts, TypeTypeEdge? ScopeRootTypeTypeEdge)> _scopeRootConfiguration = [];
    private readonly Dictionary<ScopeLevel, HashSet<ScopeNodeContext>> _scopeInstanceConfiguration = [];
    private readonly ConcurrentDictionary<int, HashSet<int>> _linkedResolutionIdsToFrom = []; 
    
    internal ITypeSymbol Type { get; } = type;
    internal IReadOnlyList<IEdge> Incoming => [.._incomingTypeEdges, .._incomingTypeTypeEdges];
    internal IReadOnlyList<TypeEdge> IncomingTypeEdges => _incomingTypeEdges;
    internal IReadOnlyList<TypeTypeEdge> IncomingTypeTypeEdges => _incomingTypeTypeEdges;

    internal IReadOnlyList<IEdge> Outgoing => ScopeRootConcreteEdge is null 
        ? OutgoingConcreteEdges 
        : [..OutgoingConcreteEdges, .._scopeRootConfiguration.Values.Select(c => c.ScopeRootTypeTypeEdge).OfType<TypeTypeEdge>()];
    internal IReadOnlyList<ConcreteEdge> OutgoingConcreteEdges => _outgoingConcreteEdges;
    internal ConcreteEdge? ScopeRootConcreteEdge { get; set; }
    /// <summary>
    /// Use scope root context (key) on all current contexts (value collection).
    /// </summary>
    internal IReadOnlyDictionary<ScopeNodeContext, (HashSet<ScopeNodeContext> PreviousScopeNodeContexts, TypeTypeEdge? ScopeRootTypeTypeEdge)> ScopeRootConfiguration => _scopeRootConfiguration;
    /// <summary>
    /// Use scope instance level (key; None means "not a scope instance") on all current contexts (value collection).
    /// </summary>
    internal IReadOnlyDictionary<ScopeLevel, HashSet<ScopeNodeContext>> ScopeInstanceConfiguration => _scopeInstanceConfiguration;
    
    internal IReadOnlyDictionary<int, HashSet<int>> LinkedResolutionIdsToFrom => _linkedResolutionIdsToFrom;
    
    internal ITypeNodeFunction? SyncFunction { get; set; }
    
    internal ITypeNodeFunction? AsyncFunction { get; set; }
    
    internal void AddIncomingTypeEdge(TypeEdge edge) => 
        _incomingTypeEdges.Add(edge);
    internal void AddIncomingTypeTypeEdge(TypeTypeEdge edge) => 
        _incomingTypeTypeEdges.Add(edge);
    internal void AddRegularOutgoing(ConcreteEdge edge) =>
        _outgoingConcreteEdges.Add(edge);
    public void RemoveEdge(IEdge edge)
    {
        if (edge is TypeEdge typeEdge)
            _incomingTypeEdges.Remove(typeEdge);
        else if (edge is ConcreteEdge concreteEdge)
            _outgoingConcreteEdges.Remove(concreteEdge);
    }
    internal EdgeContext? ContainsOutgoingEdgeFor(EdgeContext context) => 
        _outgoingConcreteEdges.SelectMany(edge => edge.Contexts).FirstOrDefault(existing => existing.Equals(context));
    internal bool TryGetOutgoingEdgeFor(IConcreteNode concreteNode, [NotNullWhen(true)] out ConcreteEdge? edge)
    {
        foreach (var e in _outgoingConcreteEdges.Where(e => Equals(e.Target, concreteNode)))
        {
            edge = e;
            return true;
        }

        edge = null;
        return false;
    }

    internal TypeTypeEdge? RegisterScopeRootConfiguration(
        ScopeNodeContext scopeRootContext, 
        ScopeNodeContext currentScopeNodeContext, 
        Func<TypeTypeEdge?> typeTypeEdgeFactory)
    {
        if (!_scopeRootConfiguration.TryGetValue(scopeRootContext, out var configuration))
        {
            var typeTypeEdge = typeTypeEdgeFactory();
            configuration = ([], typeTypeEdge);
            _scopeRootConfiguration[scopeRootContext] = configuration;
        }
        configuration.PreviousScopeNodeContexts.Add(currentScopeNodeContext);
        return configuration.ScopeRootTypeTypeEdge;
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

    public IReadOnlyList<IEdge> IncomingEdges => _incomingTypeEdges;
}