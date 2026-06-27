using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class AsyncGraphSplitter(EdgeRegistry registry)
{
    internal void Split()
    {
        var originalEdges = registry.Edges.ToImmutableArray();
        foreach (var originalEdge in originalEdges)
        {
        }
    }
}