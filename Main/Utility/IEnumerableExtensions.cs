namespace MrMeeseeks.DIE.Utility;

internal static class IEnumerableExtensions
{
    internal static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, T item, bool condition) =>
        condition ? enumerable.Append(item) : enumerable;
}