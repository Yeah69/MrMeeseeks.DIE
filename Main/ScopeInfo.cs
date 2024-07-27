namespace MrMeeseeks.DIE;

internal sealed class ScopeInfo
{
    internal ScopeInfo(
        string name,
        INamedTypeSymbol? scopeType)
    {
        Name = name;
        ScopeType = scopeType;
    }

    public string Name { get; }
    public INamedTypeSymbol? ScopeType { get; }
}