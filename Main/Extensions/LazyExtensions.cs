namespace MrMeeseeks.DIE.Extensions;

internal static class LazyExtensions
{
    internal static Lazy<TResult>
        Select<TSource, TResult>(this Lazy<TSource> source, Func<TSource, TResult> selector) =>
        new(() => selector(source.Value));
}