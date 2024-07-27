using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal sealed class EdgeRegistry : IContainerInstance
{
    private readonly List<IEdge> _edges = [];
    internal IReadOnlyList<IEdge> Edges => _edges;
    internal void Register(IEdge edge) => 
        _edges.Add(edge);
    internal void Unregister(IEdge edge) =>
        _edges.Remove(edge);
}