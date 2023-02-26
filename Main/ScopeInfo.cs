namespace MrMeeseeks.DIE;

internal interface IScopeInfo
{
    string Name { get; }
    INamedTypeSymbol? ScopeType { get; }
}

internal class ScopeInfo : IScopeInfo
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