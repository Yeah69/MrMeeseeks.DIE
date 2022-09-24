namespace MrMeeseeks.DIE;

internal interface IUserDefinedElements
{
    IFieldSymbol? GetFactoryFieldFor(ITypeSymbol type);
    IPropertySymbol? GetFactoryPropertyFor(ITypeSymbol type);
    IMethodSymbol? GetFactoryMethodFor(ITypeSymbol type);
    IMethodSymbol? AddForDisposal { get; }
    IMethodSymbol? AddForDisposalAsync { get; }
    IMethodSymbol? GetConstructorParametersInjectionFor(INamedTypeSymbol type);
    IMethodSymbol? GetPropertiesInjectionFor(INamedTypeSymbol type);
    IMethodSymbol? GetInitializerParametersInjectionFor(INamedTypeSymbol type);
}

internal class EmptyUserDefinedElements : IUserDefinedElements
{
    public IFieldSymbol? GetFactoryFieldFor(ITypeSymbol type) => null;
    public IPropertySymbol? GetFactoryPropertyFor(ITypeSymbol type) => null;
    public IMethodSymbol? GetFactoryMethodFor(ITypeSymbol type) => null;
    public IMethodSymbol? AddForDisposal => null;
    public IMethodSymbol? AddForDisposalAsync => null;
    public IMethodSymbol? GetConstructorParametersInjectionFor(INamedTypeSymbol type) => null;
    public IMethodSymbol? GetPropertiesInjectionFor(INamedTypeSymbol type) => null;
    public IMethodSymbol? GetInitializerParametersInjectionFor(INamedTypeSymbol type) => null;
}

internal class UserDefinedElements : IUserDefinedElements
{
    private readonly IReadOnlyDictionary<ISymbol?, IFieldSymbol> _typeToField;
    private readonly IReadOnlyDictionary<ISymbol?, IPropertySymbol> _typeToProperty;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _typeToMethod;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _constructorParametersInjectionMethods;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _propertiesInjectionMethods;
    private readonly IReadOnlyDictionary<ISymbol?, IMethodSymbol> _initializerParametersInjectionMethods;

    public UserDefinedElements(
        // parameter
        INamedTypeSymbol scopeType,
        INamedTypeSymbol containerType,

        // dependencies
        WellKnownTypes wellKnownTypes,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        var validationErrors = new List<Diagnostic>();
        var dieMembers = scopeType.GetMembers()
            .Where(s => s.Name.StartsWith($"{Constants.DieAbbreviation}_"))
            .ToList();

        var nonValidFactoryMembers = dieMembers
            .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory)
                        && s is IFieldSymbol or IPropertySymbol or IMethodSymbol)
            .GroupBy(s => s switch
            {
                IFieldSymbol fs => fs.Type,
                IPropertySymbol ps => ps.Type,
                IMethodSymbol ms => ms.ReturnType,
                _ => throw new ImpossibleDieException(new Guid("B75E24B2-61A3-4C37-B5A5-C7E6D390279D"))
            }, SymbolEqualityComparer.Default)
            .Where(g => g.Count() > 1)
            .ToImmutableArray();

        if (nonValidFactoryMembers.Any())
        {
            foreach (var nonValidFactoryMemberGroup in nonValidFactoryMembers)
                foreach (var symbol in nonValidFactoryMemberGroup)
                    validationErrors.Add(
                        Diagnostics.ValidationUserDefinedElement(
                            symbol, 
                            scopeType, 
                            containerType,
                            "Multiple user-defined factories aren't allowed to have the same type.",
                            ExecutionPhase.Validation));

            _typeToField = new Dictionary<ISymbol?, IFieldSymbol>(SymbolEqualityComparer.Default);
        
            _typeToProperty = new Dictionary<ISymbol?, IPropertySymbol>(SymbolEqualityComparer.Default);

            _typeToMethod = new Dictionary<ISymbol?, IMethodSymbol>(SymbolEqualityComparer.Default);
        }
        else
        {
            _typeToField = dieMembers
                .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                .OfType<IFieldSymbol>()
                .ToDictionary(fs => fs.Type, fs => fs, SymbolEqualityComparer.IncludeNullability);
        
            _typeToProperty = dieMembers
                .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                .Where(s => s is IPropertySymbol { GetMethod: { } })
                .OfType<IPropertySymbol>()
                .ToDictionary(ps => ps.Type, ps => ps, SymbolEqualityComparer.IncludeNullability);
        
            _typeToMethod = dieMembers
                .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                .Where(s => s is IMethodSymbol { ReturnsVoid: false, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary })
                .OfType<IMethodSymbol>()
                .ToDictionary(ps => ps.ReturnType, ps => ps, SymbolEqualityComparer.IncludeNullability);
        }

        AddForDisposal = dieMembers
            .Where(s => s is IMethodSymbol
            {
                DeclaredAccessibility: Accessibility.Private,
                Arity: 0,
                ReturnsVoid: true,
                IsPartialDefinition: true,
                Name: Constants.UserDefinedAddForDisposal,
                Parameters.Length: 1
            } method && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wellKnownTypes.Disposable))
            .OfType<IMethodSymbol>()
            .FirstOrDefault();
        
        AddForDisposalAsync = dieMembers
            .Where(s => s is IMethodSymbol
            {
                DeclaredAccessibility: Accessibility.Private,
                Arity: 0,
                ReturnsVoid: true,
                IsPartialDefinition: true,
                Name: Constants.UserDefinedAddForDisposalAsync,
                Parameters.Length: 1
            } method && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wellKnownTypes.AsyncDisposable))
            .OfType<IMethodSymbol>()
            .FirstOrDefault();

        _constructorParametersInjectionMethods = GetInjectionMethods(Constants.UserDefinedConstrParams, wellKnownTypesMiscellaneous.UserDefinedConstructorParametersInjectionAttribute);
        
        _propertiesInjectionMethods = GetInjectionMethods(Constants.UserDefinedProps, wellKnownTypesMiscellaneous.UserDefinedPropertiesInjectionAttribute);
        
        _initializerParametersInjectionMethods = GetInjectionMethods(Constants.UserDefinedInitParams, wellKnownTypesMiscellaneous.UserDefinedInitializerParametersInjectionAttribute);

        if (validationErrors.Any())
            throw new ValidationDieException(validationErrors.ToImmutableArray());

        IReadOnlyDictionary<ISymbol?, IMethodSymbol> GetInjectionMethods(string prefix, INamedTypeSymbol attributeType)
        {
            var injectionMethodCandidates = dieMembers
            .Where(s => s.Name.StartsWith(prefix))
            .Where(s => s is IMethodSymbol { ReturnsVoid: true, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary } method
                        && method.Parameters.Any(p => p.RefKind == RefKind.Out))
            .OfType<IMethodSymbol>()
            .Select(m =>
            {
                var type = m.GetAttributes()
                    .Where(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeType))
                    .Select(ad =>
                    {
                        if (ad.ConstructorArguments.Length != 1)
                            return null;
                        return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                    })
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault();

                return type is { } ? (type, m) : ((INamedTypeSymbol, IMethodSymbol)?) null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>()
            .ToImmutableArray();

            var injectionMethodGroupings = injectionMethodCandidates
                .GroupBy(t => t.Item1, SymbolEqualityComparer.Default)
                .Where(g => g.Count() > 1)
                .ToImmutableArray();
            
            if (injectionMethodGroupings.Any())
            {
                foreach (var nonValidInjectionMethodGroup in injectionMethodGroupings)
                    foreach (var t in nonValidInjectionMethodGroup)
                        validationErrors.Add(
                            Diagnostics.ValidationUserDefinedElement(
                                t.Item2, 
                                scopeType, 
                                containerType,
                                "Multiple user-defined custom constructor parameter methods aren't allowed to have the same type that they are based on.",
                                ExecutionPhase.Validation));

                return new Dictionary<ISymbol?, IMethodSymbol>(SymbolEqualityComparer.Default);
            }
            
            return injectionMethodCandidates
                .OfType<(INamedTypeSymbol, IMethodSymbol)>()
                .ToDictionary(t => t.Item1, t => t.Item2, SymbolEqualityComparer.Default);
        }
    }

    public IFieldSymbol? GetFactoryFieldFor(ITypeSymbol typeSymbol) => 
        _typeToField.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IPropertySymbol? GetFactoryPropertyFor(ITypeSymbol typeSymbol) => 
        _typeToProperty.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IMethodSymbol? GetFactoryMethodFor(ITypeSymbol typeSymbol) => 
        _typeToMethod.TryGetValue(typeSymbol, out var ret) ? ret : null;

    public IMethodSymbol? AddForDisposal { get; }
    public IMethodSymbol? AddForDisposalAsync { get; }
    public IMethodSymbol? GetConstructorParametersInjectionFor(INamedTypeSymbol type) => 
        _constructorParametersInjectionMethods.TryGetValue(type, out var ret) ? ret : null;

    public IMethodSymbol? GetPropertiesInjectionFor(INamedTypeSymbol type) =>
        _propertiesInjectionMethods.TryGetValue(type, out var ret) ? ret : null;

    public IMethodSymbol? GetInitializerParametersInjectionFor(INamedTypeSymbol type) => 
        _initializerParametersInjectionMethods.TryGetValue(type, out var ret) ? ret : null;
}