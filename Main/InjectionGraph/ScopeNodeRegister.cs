using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ScopeNodeRegister : IContainerInstance
{
    private readonly Func<ScopeNode> _nodeFactory;
    private readonly Dictionary<string, ScopeNode> _nodes = [];

    internal ScopeNodeRegister(Func<ScopeNode>  nodeFactory)
    {
        _nodeFactory = nodeFactory;
        ContainerScopeNode = nodeFactory();
    }

    internal ScopeNode ContainerScopeNode { get; }
    internal ScopeNode this[string name] => _nodes[name];

    internal void RegisterContainerInstance(TypeNode node) =>
        ContainerScopeNode.AddScopedInstance(node);

    internal void RegisterScopedInstance(string scopeName, TypeNode node)
    {
        if (!_nodes.TryGetValue(scopeName, out var scopedInstances))
        {
            scopedInstances = _nodeFactory();
            _nodes[scopeName] = scopedInstances;
        }
        scopedInstances.AddScopedInstance(node);
    }
}