namespace MrMeeseeks.DIE;

internal interface IUserProvidedScopeElements
{
    IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol);
    IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol);
    IMethodSymbol? GetFactoryFor(ITypeSymbol typeSymbol);
    IMethodSymbol? AddForDisposal { get; }
    IMethodSymbol? AddForDisposalAsync { get; }
}

internal class EmptyUserProvidedScopeElements : IUserProvidedScopeElements
{
    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => null;
    public IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol) => null;
    public IMethodSymbol? GetFactoryFor(ITypeSymbol typeSymbol) => null;
    public IMethodSymbol? AddForDisposal => null;
    public IMethodSymbol? AddForDisposalAsync => null;
}

internal class UserProvidedScopeElements : IUserProvidedScopeElements
{
    private readonly IReadOnlyDictionary<ISymbol?, IFieldSymbol> _typeToField;
    private readonly IReadOnlyDictionary<ISymbol?, IPropertySymbol> _typeToProperty;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _typeToMethod;

    public UserProvidedScopeElements(
        // parameter
        INamedTypeSymbol scopeType,
        
        // dependencies
        WellKnownTypes wellKnownTypes)
    {
        var dieMembers = scopeType.GetMembers()
            .Where(s => s.Name.StartsWith("DIE_"))
            .ToList();

        _typeToField = dieMembers
            .Where(s => s is IFieldSymbol)
            .OfType<IFieldSymbol>()
            .Where(f => f.Name.StartsWith("DIE_Factory_"))
            .ToDictionary(fs => fs.Type, fs => fs, SymbolEqualityComparer.IncludeNullability);
        
        _typeToProperty = dieMembers
            .Where(s => s is IPropertySymbol { GetMethod: { } })
            .OfType<IPropertySymbol>()
            .Where(f => f.Name.StartsWith("DIE_Factory_"))
            .ToDictionary(ps => ps.Type, ps => ps, SymbolEqualityComparer.IncludeNullability);
        
        _typeToMethod = dieMembers
            .Where(s => s is IMethodSymbol { ReturnsVoid: false, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary })
            .OfType<IMethodSymbol>()
            .Where(f => f.Name.StartsWith("DIE_Factory_"))
            .ToDictionary(ps => ps.ReturnType, ps => ps, SymbolEqualityComparer.IncludeNullability);

        AddForDisposal = dieMembers
            .Where(s => s is IMethodSymbol
            {
                DeclaredAccessibility: Accessibility.Private,
                ReturnsVoid: true,
                IsPartialDefinition: true,
                Name: "DIE_AddForDisposal",
                Parameters.Length: 1
            } method && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wellKnownTypes.Disposable))
            .OfType<IMethodSymbol>()
            .FirstOrDefault();
        
        AddForDisposalAsync = dieMembers
            .Where(s => s is IMethodSymbol
            {
                DeclaredAccessibility: Accessibility.Private,
                ReturnsVoid: true,
                IsPartialDefinition: true,
                Name: "DIE_AddForDisposalAsync",
                Parameters.Length: 1
            } method && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wellKnownTypes.AsyncDisposable))
            .OfType<IMethodSymbol>()
            .FirstOrDefault();
    }

    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => 
        _typeToField.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol) => 
        _typeToProperty.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IMethodSymbol? GetFactoryFor(ITypeSymbol typeSymbol) => 
        _typeToMethod.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IMethodSymbol? AddForDisposal { get; }
    public IMethodSymbol? AddForDisposalAsync { get; }
}