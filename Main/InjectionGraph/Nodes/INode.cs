using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface INode
{
    IReadOnlyList<IEdge> IncomingEdges { get; }   
    void RemoveEdge(IEdge edge);
}