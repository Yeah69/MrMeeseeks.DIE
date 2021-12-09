namespace MrMeeseeks.DIE;

internal interface IUserProvidedScopeElements
{
    IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol);
}

internal class EmptyUserProvidedScopeElements : IUserProvidedScopeElements
{
    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => null;
}

internal class UserProvidedScopeElements : IUserProvidedScopeElements
{
    private readonly IReadOnlyDictionary<ISymbol?, IFieldSymbol> _typeToField;

    public UserProvidedScopeElements(INamedTypeSymbol scopeType)
    {
        _typeToField = scopeType.GetMembers()
            .Where(s => s.Name.StartsWith("DIE_"))
            .Where(s => s is IFieldSymbol fieldSymbol && (fieldSymbol.IsConst || fieldSymbol.IsReadOnly))
            .OfType<IFieldSymbol>()
            .ToDictionary(fs => fs.Type, fs => fs, SymbolEqualityComparer.IncludeNullability);
    }

    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => 
        _typeToField.TryGetValue(typeSymbol, out var ret) ? ret : null;
}