namespace MrMeeseeks.DIE.Utility;

internal class ValueTupleEqualityComparer<T1, T2>(
    IEqualityComparer<T1> t1EqualityComparer,
    IEqualityComparer<T2> t2EqualityComparer)
    : IEqualityComparer<ValueTuple<T1, T2>>
{
    public bool Equals((T1, T2) x, (T1, T2) y)
    {
        if (!t1EqualityComparer.Equals(x.Item1, y.Item1))
            return false;
        if (!t2EqualityComparer.Equals(x.Item2, y.Item2))
            return false;
        return true;
    }

    public int GetHashCode((T1, T2) obj)
    {
        var hash = new HashCode();
        hash.Add(obj.Item1, t1EqualityComparer);
        hash.Add(obj.Item2, t2EqualityComparer);
        return hash.ToHashCode();
    }
}