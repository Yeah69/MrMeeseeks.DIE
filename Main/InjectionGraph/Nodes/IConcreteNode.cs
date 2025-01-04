using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface IConcreteNode
{
    IReadOnlyList<TypeNode> ConnectIfNotAlready(EdgeContext context);
}