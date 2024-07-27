using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal interface IEdge
{
    IReadOnlyList<EdgeContext> Contexts { get; }
    INode SourceAsNode { get; }
    INode TargetAsNode { get; }
}