using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Extensions;

// ReSharper disable once InconsistentNaming
internal static class TExtensions
{
    internal static T EnqueueBuildJobTo<T>(this T item, Queue<BuildJob> queue, PassedContext passedContext) 
        where T : INode
    {
        var buildJob = new BuildJob(item, passedContext);
        queue.Enqueue(buildJob);
        return item;
    }
}