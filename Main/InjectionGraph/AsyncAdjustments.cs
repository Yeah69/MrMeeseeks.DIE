using System.Collections.Concurrent;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class AsyncAdjustments(EdgeRegistry edgeRegistry) : IScopeInstance
{
    private readonly ConcurrentDictionary<INode, HashSet<int>> _awaitedNodes = [];
    
    internal void AddAwaitedNode(IConcreteNode node, int resolutionId) =>
        _awaitedNodes.GetOrAdd(node, _ => []).Add(resolutionId);
    
    internal ConcurrentDictionary<INode, HashSet<int>> AwaitedNodes => 
        _awaitedNodes;

    internal void Adjust()
    {
        EnhanceAwaitedNodes();
        var set = new HashSet<int>();
        foreach (var resolutionIds in _awaitedNodes.Select(kvp => kvp.Value))
            set.UnionWith(resolutionIds);
        foreach (var edge in edgeRegistry.Edges)
            edge.AsyncAdjust(set);
    }

    private void EnhanceAwaitedNodes()
    {
        var outerQueue = new Queue<(INode, int)>(_awaitedNodes.SelectMany(kvp => kvp.Value.Select(ri => (kvp.Key, ri))));
        while (outerQueue.Count > 0)
        {
            var (outerNode, resolutionId) = outerQueue.Dequeue();
            var innerQueue = new Queue<INode>();
            innerQueue.Enqueue(outerNode);
            
            var visitedNodes = new HashSet<INode>();
            while (innerQueue.Count > 0)
            {
                var currentNode = innerQueue.Dequeue();
                visitedNodes.Add(currentNode);
                if (currentNode is IConcreteNode currentConcreteNode)
                {
                    var sequence = currentConcreteNode
                        .IncomingEdges
                        .Where(e => e.Contexts.Any(c => c.ResolutionId == resolutionId))
                        .OfType<ConcreteEdge>()
                        .Select(e => e.Source)
                        .Where(n => !visitedNodes.Contains(n));
                    foreach (var typeNode in sequence)
                        innerQueue.Enqueue(typeNode);
                } 
                else if (currentNode is TypeNode currentTypeNode)
                {
                    var sequence = currentTypeNode
                        .IncomingEdges
                        .Where(e => e.Contexts.Any(c => c.ResolutionId == resolutionId))
                        .OfType<TypeEdge>()
                        .Select(e => e.Source)
                        .Where(n => !visitedNodes.Contains(n) && n is not ConcreteFunctorNode);
                    foreach (var concreteNode in sequence)
                        innerQueue.Enqueue(concreteNode);

                    if (currentTypeNode.LinkedResolutionIdsToFrom.TryGetValue(resolutionId, out var linkedResolutionIds))
                        foreach (var linkedResolutionId in linkedResolutionIds)
                            _awaitedNodes.GetOrAdd(currentTypeNode, _ => []).Add(linkedResolutionId);
                }
            }
        }
    }
}