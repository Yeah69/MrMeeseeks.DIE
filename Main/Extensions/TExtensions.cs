namespace MrMeeseeks.DIE.Extensions;

internal static class TExtensions
{
    internal static T EnqueueTo<T, TQueue>(this T item, Queue<TQueue> queue) where T : TQueue
    {
        queue.Enqueue(item);
        return item;
    }
}