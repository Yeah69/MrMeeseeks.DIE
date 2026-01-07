using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class NodesRegister : IContainerInstance
{
    private readonly Func<Node> _nodeFactory;
    private readonly Dictionary<string, Node> _nodes = [];

    internal NodesRegister(Func<Node>  nodeFactory)
    {
        _nodeFactory = nodeFactory;
        ContainerNode = nodeFactory();
    }

    internal Node ContainerNode { get; }
    internal Node this[string name] => _nodes[name];

    internal void RegisterContainerInstance(TypeNode node) =>
        ContainerNode.AddScopedInstance(node);

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