namespace MrMeeseeks.DIE;

internal interface IUserProvidedScopeElements
{
    IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol);
    IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol);
}

internal class EmptyUserProvidedScopeElements : IUserProvidedScopeElements
{
    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => null;
    public IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol) => null;
}

internal class UserProvidedScopeElements : IUserProvidedScopeElements
{
    private readonly IReadOnlyDictionary<ISymbol?, IFieldSymbol> _typeToField;
    private readonly IReadOnlyDictionary<ISymbol?, IPropertySymbol> _typeToProperty;

    public UserProvidedScopeElements(INamedTypeSymbol scopeType)
    {
        var dieMembers = scopeType.GetMembers()
            .Where(s => s.Name.StartsWith("DIE_"))
            .ToList();

        _typeToField = dieMembers
            .Where(s => s is IFieldSymbol fieldSymbol && (fieldSymbol.IsConst || fieldSymbol.IsReadOnly))
            .OfType<IFieldSymbol>()
            .ToDictionary(fs => fs.Type, fs => fs, SymbolEqualityComparer.IncludeNullability);
        
        _typeToProperty = dieMembers
            .Where(s => s is IPropertySymbol { GetMethod: { } })
            .OfType<IPropertySymbol>()
            .ToDictionary(ps => ps.Type, ps => ps, SymbolEqualityComparer.IncludeNullability);
    }

    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => 
        _typeToField.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol) => 
        _typeToProperty.TryGetValue(typeSymbol, out var ret) ? ret : null;
}