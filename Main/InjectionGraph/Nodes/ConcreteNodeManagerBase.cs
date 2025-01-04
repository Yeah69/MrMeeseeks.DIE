using System.Diagnostics.CodeAnalysis;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal abstract class ConcreteNodeManagerBase<TData, TConcreteNode>(Func<TData, TConcreteNode> factory)
{
    private readonly Dictionary<TData, TConcreteNode> _nodes = [];
    
    internal TConcreteNode GetOrAddNode(TData data)
    {
        if (_nodes.TryGetValue(data, out var node))
            return node;
        node = factory(data);
        _nodes[data] = node;
        return node;
    }
    
    internal bool TryGetNode(TData data, [NotNullWhen(true)] out TConcreteNode? node) => _nodes.TryGetValue(data, out node);
}