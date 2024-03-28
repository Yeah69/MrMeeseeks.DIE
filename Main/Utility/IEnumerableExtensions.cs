namespace MrMeeseeks.DIE.Utility;

// ReSharper disable once InconsistentNaming
internal static class IEnumerableExtensions
{
    internal static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, T item, bool condition) =>
        condition ? enumerable.Append(item) : enumerable;
}