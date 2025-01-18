using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface IConcreteNode
{
    IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context);
}