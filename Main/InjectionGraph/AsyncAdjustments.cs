using System.Collections.Concurrent;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class AsyncAdjustments : IScopeInstance
{
    private readonly ConcurrentDictionary<INode, HashSet<int>> _awaitedNodes = [];
    
    internal void AddAwaitedNode(IConcreteNode node, int resolutionId) =>
        _awaitedNodes.GetOrAdd(node, _ => []).Add(resolutionId);
    
    internal ConcurrentDictionary<INode, HashSet<int>> AwaitedNodes => 
        _awaitedNodes;
}