using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface IConcreteNode : INode
{
    void AddIncomingEdge(IEdge edge);
}