using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal abstract class ConcreteNodeBase : IConcreteNode
{
    private readonly List<IEdge> _incomingEdges = [];
    public IReadOnlyList<IEdge> IncomingEdges => _incomingEdges;

    public void AddIncomingEdge(IEdge edge) => 
        _incomingEdges.Add(edge);
    public void RemoveEdge(IEdge edge) => 
        _incomingEdges.Remove(edge);
}