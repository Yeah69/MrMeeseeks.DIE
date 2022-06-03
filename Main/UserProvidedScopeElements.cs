namespace MrMeeseeks.DIE;

internal interface IUserProvidedScopeElements
{
    IFieldSymbol? GetInstanceFor(ITypeSymbol type);
    IPropertySymbol? GetPropertyFor(ITypeSymbol type);
    IMethodSymbol? GetFactoryFor(ITypeSymbol type);
    IMethodSymbol? AddForDisposal { get; }
    IMethodSymbol? AddForDisposalAsync { get; }
    IMethodSymbol? GetCustomConstructorParameterChoiceFor(INamedTypeSymbol type);
}

internal class EmptyUserProvidedScopeElements : IUserProvidedScopeElements
{
    public IFieldSymbol? GetInstanceFor(ITypeSymbol type) => null;
    public IPropertySymbol? GetPropertyFor(ITypeSymbol type) => null;
    public IMethodSymbol? GetFactoryFor(ITypeSymbol type) => null;
    public IMethodSymbol? AddForDisposal => null;
    public IMethodSymbol? AddForDisposalAsync => null;
    public IMethodSymbol? GetCustomConstructorParameterChoiceFor(INamedTypeSymbol type) => null;
}

internal class UserProvidedScopeElements : IUserProvidedScopeElements
{
    private readonly IReadOnlyDictionary<ISymbol?, IFieldSymbol> _typeToField;
    private readonly IReadOnlyDictionary<ISymbol?, IPropertySymbol> _typeToProperty;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _typeToMethod;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _customConstructorParameterChoiceMethods;

    public UserProvidedScopeElements(
        // parameter
        INamedTypeSymbol scopeType,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        WellKnownTypesChoice wellKnownTypesChoice)
    {
        var dieMembers = scopeType.GetMembers()
            .Where(s => s.Name.StartsWith("DIE_"))
            .ToList();

        _typeToField = dieMembers
            .Where(s => s.Name.StartsWith("DIE_Factory_"))
            .OfType<IFieldSymbol>()
            .ToDictionary(fs => fs.Type, fs => fs, SymbolEqualityComparer.IncludeNullability);
        
        _typeToProperty = dieMembers
            .Where(s => s.Name.StartsWith("DIE_Factory_"))
            .Where(s => s is IPropertySymbol { GetMethod: { } })
            .OfType<IPropertySymbol>()
            .ToDictionary(ps => ps.Type, ps => ps, SymbolEqualityComparer.IncludeNullability);
        
        _typeToMethod = dieMembers
            .Where(s => s.Name.StartsWith("DIE_Factory_"))
            .Where(s => s is IMethodSymbol { ReturnsVoid: false, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary })
            .OfType<IMethodSymbol>()
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
        
        _customConstructorParameterChoiceMethods = dieMembers
            .Where(s => s.Name.StartsWith("DIE_ConstrParam_"))
            .Where(s => s is IMethodSymbol { ReturnsVoid: true, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary } method
                && method.Parameters.Any(p => p.RefKind == RefKind.Out))
            .OfType<IMethodSymbol>()
            .Select(m =>
            {
                var type = m.GetAttributes()
                    .Where(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        wellKnownTypesChoice.CustomConstructorParameterChoiceAttribute))
                    .Select(ad =>
                    {
                        if (ad.ConstructorArguments.Length != 1)
                            return null;
                        return ad.ConstructorArguments[0].Value is INamedTypeSymbol type ? type : null;
                    })
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault();

                return type is { } ? (type, m) : ((INamedTypeSymbol, IMethodSymbol)?) null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>()
            .ToDictionary(t => t.Item1, t => t.Item2, SymbolEqualityComparer.Default);
    }

    public IFieldSymbol? GetInstanceFor(ITypeSymbol typeSymbol) => 
        _typeToField.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IPropertySymbol? GetPropertyFor(ITypeSymbol typeSymbol) => 
        _typeToProperty.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IMethodSymbol? GetFactoryFor(ITypeSymbol typeSymbol) => 
        _typeToMethod.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IMethodSymbol? AddForDisposal { get; }
    public IMethodSymbol? AddForDisposalAsync { get; }
    public IMethodSymbol? GetCustomConstructorParameterChoiceFor(INamedTypeSymbol type) => 
        _customConstructorParameterChoiceMethods.TryGetValue(type, out var ret) ? ret : null;
}