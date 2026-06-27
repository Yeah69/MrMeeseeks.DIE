using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal interface IEdge
{
    IReadOnlyList<EdgeContext> Contexts { get; }
    IReadOnlyList<EdgeContext> AsyncContexts { get; }
    INode SourceAsNode { get; }
    INode TargetAsNode { get; }
    void AsyncAdjust(HashSet<int> asyncResolutionIds);
}