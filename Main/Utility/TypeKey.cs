namespace MrMeeseeks.DIE.Utility;

internal record TypeKey(string Value) : IComparable<TypeKey>
{
    public int CompareTo(TypeKey? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return string.Compare(Value, other.Value, StringComparison.Ordinal);
    }
}