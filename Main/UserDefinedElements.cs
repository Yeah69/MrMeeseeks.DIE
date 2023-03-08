using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility;

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

internal class UserDefinedElements : IUserDefinedElements
{
    private readonly IReadOnlyDictionary<ITypeSymbol, IFieldSymbol> _typeToField;
    private readonly IReadOnlyDictionary<ITypeSymbol, IPropertySymbol> _typeToProperty;
    private readonly IReadOnlyDictionary<ITypeSymbol, IMethodSymbol> _typeToMethod;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> _constructorParametersInjectionMethods;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> _propertiesInjectionMethods;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> _initializerParametersInjectionMethods;

    public UserDefinedElements(
        // parameter
        (INamedTypeSymbol? Range, INamedTypeSymbol Container) types,

        // dependencies
        IContainerWideContext containerWideContext)
    {
        if (types.Range is { } range)
        {
            var validationErrors = new List<Diagnostic>();
            var dieMembers = range.GetMembers()
                .Where(s => s.Name.StartsWith($"{Constants.DieAbbreviation}_"))
                .ToList();

            var wellKnownTypes = containerWideContext.WellKnownTypes;
            var nonValidFactoryMembers = dieMembers
                .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory)
                            && s is IFieldSymbol or IPropertySymbol or IMethodSymbol)
                .GroupBy(s =>
                {
                    var outerType = s switch
                    {
                        IFieldSymbol fs => fs.Type,
                        IPropertySymbol ps => ps.Type,
                        IMethodSymbol ms => ms.ReturnType,
                        _ => throw new ImpossibleDieException(new Guid("B75E24B2-61A3-4C37-B5A5-C7E6D390279D"))
                    };
                    return GetAsyncUnwrappedType(outerType, wellKnownTypes);
                }, CustomSymbolEqualityComparer.IncludeNullability)
                .Where(g => g.Count() > 1)
                .ToImmutableArray();

            if (nonValidFactoryMembers.Any())
            {
                foreach (var nonValidFactoryMemberGroup in nonValidFactoryMembers)
                    foreach (var symbol in nonValidFactoryMemberGroup)
                        validationErrors.Add(
                            Diagnostics.ValidationUserDefinedElement(
                                symbol, 
                                range, 
                                types.Container,
                                "Multiple user-defined factories aren't allowed to have the same type.",
                                ExecutionPhase.Validation));

                _typeToField = new Dictionary<ITypeSymbol, IFieldSymbol>(CustomSymbolEqualityComparer.IncludeNullability);
            
                _typeToProperty = new Dictionary<ITypeSymbol, IPropertySymbol>(CustomSymbolEqualityComparer.IncludeNullability);

                _typeToMethod = new Dictionary<ITypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.IncludeNullability);
            }
            else
            {
                _typeToField = dieMembers
                    .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                    .OfType<IFieldSymbol>()
                    .ToDictionary<IFieldSymbol, ITypeSymbol, IFieldSymbol>(
                        fs => GetAsyncUnwrappedType(fs.Type, wellKnownTypes), 
                        fs => fs,
                        CustomSymbolEqualityComparer.IncludeNullability);
            
                _typeToProperty = dieMembers
                    .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                    .Where(s => s is IPropertySymbol { GetMethod: { } })
                    .OfType<IPropertySymbol>()
                    .ToDictionary<IPropertySymbol, ITypeSymbol, IPropertySymbol>(
                        ps => GetAsyncUnwrappedType(ps.Type, wellKnownTypes), 
                        ps => ps,
                        CustomSymbolEqualityComparer.IncludeNullability);
            
                _typeToMethod = dieMembers
                    .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                    .Where(s => s is IMethodSymbol { ReturnsVoid: false, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary })
                    .OfType<IMethodSymbol>()
                    .ToDictionary<IMethodSymbol, ITypeSymbol, IMethodSymbol>(
                        ms => GetAsyncUnwrappedType(ms.ReturnType, wellKnownTypes), 
                        ms => ms,
                        CustomSymbolEqualityComparer.IncludeNullability);
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
                } method && CustomSymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wellKnownTypes.IDisposable))
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
                } method && CustomSymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wellKnownTypes.IAsyncDisposable))
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            var wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
            _constructorParametersInjectionMethods = GetInjectionMethods(Constants.UserDefinedConstrParams, wellKnownTypesMiscellaneous.UserDefinedConstructorParametersInjectionAttribute);
            
            _propertiesInjectionMethods = GetInjectionMethods(Constants.UserDefinedProps, wellKnownTypesMiscellaneous.UserDefinedPropertiesInjectionAttribute);
            
            _initializerParametersInjectionMethods = GetInjectionMethods(Constants.UserDefinedInitParams, wellKnownTypesMiscellaneous.UserDefinedInitializerParametersInjectionAttribute);

            if (validationErrors.Any())
                throw new ValidationDieException(validationErrors.ToImmutableArray());

            IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> GetInjectionMethods(string prefix, INamedTypeSymbol attributeType)
            {
                var injectionMethodCandidates = dieMembers
                .Where(s => s.Name.StartsWith(prefix))
                .Where(s => s is IMethodSymbol { ReturnsVoid: true, Arity: 0, IsConditional: false, MethodKind: MethodKind.Ordinary } method
                            && method.Parameters.Any(p => p.RefKind == RefKind.Out))
                .OfType<IMethodSymbol>()
                .Select(m =>
                {
                    var type = m.GetAttributes()
                        .Where(ad => CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeType))
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
                    .GroupBy(t => t.Item1, CustomSymbolEqualityComparer.Default)
                    .Where(g => g.Count() > 1)
                    .ToImmutableArray();
                
                if (injectionMethodGroupings.Any())
                {
                    foreach (var nonValidInjectionMethodGroup in injectionMethodGroupings)
                        foreach (var t in nonValidInjectionMethodGroup)
                            validationErrors.Add(
                                Diagnostics.ValidationUserDefinedElement(
                                    t.Item2, 
                                    range, 
                                    types.Container,
                                    "Multiple user-defined custom constructor parameter methods aren't allowed to have the same type that they are based on.",
                                    ExecutionPhase.Validation));

                    return new Dictionary<INamedTypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.Default);
                }
                
                return injectionMethodCandidates
                    .OfType<(INamedTypeSymbol, IMethodSymbol)>()
                    .ToDictionary<(INamedTypeSymbol, IMethodSymbol), INamedTypeSymbol, IMethodSymbol>(
                        t => t.Item1,
                        t => t.Item2,
                        CustomSymbolEqualityComparer.Default);
            }
            
            static ITypeSymbol GetAsyncUnwrappedType(ITypeSymbol type, WellKnownTypes wellKnownTypes)
            {
                if ((CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ValueTask1)
                     || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Task1))
                    && type is INamedTypeSymbol namedType)
                    return namedType.TypeArguments.First();

                return type;
            }
        }
        else
        {
            _typeToField = new Dictionary<ITypeSymbol, IFieldSymbol>(CustomSymbolEqualityComparer.Default);
            _typeToProperty = new Dictionary<ITypeSymbol, IPropertySymbol>(CustomSymbolEqualityComparer.Default);
            _typeToMethod = new Dictionary<ITypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.Default);
            _constructorParametersInjectionMethods = new Dictionary<INamedTypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.Default);
            _propertiesInjectionMethods = new Dictionary<INamedTypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.Default);
            _initializerParametersInjectionMethods = new Dictionary<INamedTypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.Default);
            AddForDisposal = null;
            AddForDisposalAsync = null;
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