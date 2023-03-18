using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Extensions;

// ReSharper disable once InconsistentNaming
internal static class TExtensions
{
    internal static T EnqueueBuildJobTo<T>(this T item, Queue<BuildJob> queue, ImmutableStack<INamedTypeSymbol> implementationSet) 
        where T : INode
    {
        var buildJob = new BuildJob(item, implementationSet);
        queue.Enqueue(buildJob);
        return item;
    }
}