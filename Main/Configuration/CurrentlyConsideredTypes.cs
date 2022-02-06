using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MrMeeseeks.DIE.Configuration;

internal interface ICurrentlyConsideredTypes
{
    IImmutableSet<ISymbol?> TransientTypes { get; }
    IImmutableSet<ISymbol?> ContainerInstanceTypes { get; }
    IImmutableSet<ISymbol?> TransientScopeInstanceTypes { get; }
    IImmutableSet<ISymbol?> ScopeInstanceTypes { get; }
    IImmutableSet<ISymbol?> TransientScopeRootTypes { get; }
    IImmutableSet<ISymbol?> ScopeRootTypes { get; }
    IReadOnlyDictionary<ISymbol?, INamedTypeSymbol> InterfaceToComposite { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> ImplementationToConstructorChoice { get; }
    IReadOnlyDictionary<ISymbol?, IReadOnlyList<INamedTypeSymbol>> InterfaceToDecorators { get; }
    IReadOnlyDictionary<INamedTypeSymbol,IReadOnlyList<INamedTypeSymbol>> InterfaceSequenceChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol,IReadOnlyList<INamedTypeSymbol>> ImplementationSequenceChoices { get; }
    IReadOnlyDictionary<ITypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationMap { get; }
    IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
}

internal class CurrentlyConsideredTypes : ICurrentlyConsideredTypes
{
    public CurrentlyConsideredTypes(
        IReadOnlyList<ITypesFromAttributes> typesFromAttributes,
        GeneratorExecutionContext context)
    {
        var allImplementations = new List<INamedTypeSymbol>();
        
        allImplementations.AddRange(context.Compilation.SyntaxTrees
            .Select(st => (st, context.Compilation.GetSemanticModel(st)))
            .SelectMany(t => t.st
                .GetRoot()
                .DescendantNodesAndSelf()
                .Where(e => e is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax)
                .Select(c => t.Item2.GetDeclaredSymbol(c))
                .Where(c => c is not null)
                .OfType<INamedTypeSymbol>()));
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterType in types.FilterSpy.Concat(types.FilterImplementation))
                allImplementations.Remove(filterType);
            
            var spiedImplementations = types
                .Spy
                .SelectMany(t => t?.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(ms => !ms.ReturnsVoid)
                    .Select(ms => ms.ReturnType)
                    .OfType<INamedTypeSymbol>());

            allImplementations.AddRange(types.Implementation
                .Concat(spiedImplementations));
        }

        TransientTypes = GetSetOfTypesWithProperties(t => t.Transient, t => t.FilterTransient);
        ContainerInstanceTypes = GetSetOfTypesWithProperties(t => t.ContainerInstance, t => t.FilterContainerInstance);
        TransientScopeInstanceTypes = GetSetOfTypesWithProperties(t => t.TransientScopeInstance, t => t.FilterTransientScopeInstance);
        ScopeInstanceTypes = GetSetOfTypesWithProperties(t => t.ScopeInstance, t => t.FilterScopeInstance);
        TransientScopeRootTypes = GetSetOfTypesWithProperties(t => t.TransientScopeRoot, t => t.FilterTransientScopeRoot);
        ScopeRootTypes = GetSetOfTypesWithProperties(t => t.ScopeRoot, t => t.FilterScopeRoot);

        var compositeInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            compositeInterfaces = compositeInterfaces.Except(types.FilterComposite);
            compositeInterfaces = compositeInterfaces.Union(types.Composite);
        }
        
        var compositeTypes = GetSetOfTypesWithProperties(t => t.Composite, t => t.FilterComposite);
        InterfaceToComposite = compositeTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.AllInterfaces
                    .Single(t => compositeInterfaces.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.First();
            }, SymbolEqualityComparer.Default)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single(), SymbolEqualityComparer.Default);

        var constructorChoices = new Dictionary<INamedTypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterConstructorChoice in types.FilterConstructorChoices)
                constructorChoices.Remove(filterConstructorChoice);

            foreach (var (implementationType, constructor) in types.ConstructorChoices)
                constructorChoices[implementationType] = constructor;
        }
        
        ImplementationToConstructorChoice = constructorChoices;
        
        var decoratorInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            decoratorInterfaces = decoratorInterfaces.Except(types.FilterDecorator);
            decoratorInterfaces = decoratorInterfaces.Union(types.Decorator);
        }
        
        var decoratorTypes = GetSetOfTypesWithProperties(t => t.Decorator, t => t.FilterDecorator);
        InterfaceToDecorators = decoratorTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.AllInterfaces
                    .Single(t => decoratorInterfaces.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.First();
            }, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<INamedTypeSymbol>) g.ToList(), SymbolEqualityComparer.Default);
        
        var decoratorSequenceChoices = new Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterDecoratorSequenceChoice in types.FilterDecoratorSequenceChoices)
                decoratorSequenceChoices.Remove(filterDecoratorSequenceChoice);

            foreach (var (decoratedType, decoratorSequence) in types.DecoratorSequenceChoices)
                decoratorSequenceChoices[decoratedType] = decoratorSequence;
        }

        InterfaceSequenceChoices = decoratorSequenceChoices
            .Where(kvp => kvp.Key.TypeKind == TypeKind.Interface)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        ImplementationSequenceChoices = decoratorSequenceChoices
            .Where(kvp => kvp.Key.TypeKind is TypeKind.Class or TypeKind.Struct)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        ImplementationMap = allImplementations
            .Where(t => !decoratorTypes.Contains(t.OriginalDefinition))
            .Where(t => !compositeTypes.Contains(t.OriginalDefinition))
            .SelectMany(i => { return i.AllInterfaces.OfType<ITypeSymbol>().Select(ii => (ii, i)).Prepend((i, i)); })
            .GroupBy(t => t.Item1, t => t.Item2)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<INamedTypeSymbol>) g.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToList());
        
        var initializers = new Dictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)>(SymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            var filterInterfaceTypes = types
                .FilterTypeInitializers
                .Where(t => t.TypeKind is TypeKind.Interface)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
            
            foreach (var filterConcreteType in allImplementations
                         .Where(i => AllDerivedTypes(i)
                             .Select(t => t.OriginalDefinition)
                             .Any(inter => filterInterfaceTypes.Contains(inter))))
                initializers.Remove(filterConcreteType);

            var filterConcreteTypes = types
                .FilterTypeInitializers
                .Where(t => t.TypeKind is TypeKind.Class or TypeKind.Struct)
                .ToList();
            
            foreach (var filterConcreteType in filterConcreteTypes)
                initializers.Remove(filterConcreteType);
            
            var interfaceTypes = types
                .TypeInitializers
                .Where(ti => ti.Item1.TypeKind is TypeKind.Interface)
                .ToList();

            foreach (var (implementationType, interfaceType, initializerMethod) in allImplementations
                         .Select(i =>
                         {
                             foreach (var (interfaceType, initializer) in interfaceTypes)
                             {
                                 if (AllDerivedTypes(i).Select(d => d.OriginalDefinition).Contains(interfaceType, SymbolEqualityComparer.Default))
                                 {
                                     return ((INamedTypeSymbol, INamedTypeSymbol, IMethodSymbol)?) (i, interfaceType, initializer);
                                 }
                             }

                             return null;
                         })
                         .OfType<(INamedTypeSymbol, INamedTypeSymbol, IMethodSymbol)>())
                initializers[implementationType] = (interfaceType, initializerMethod);

            var concreteTypes = types
                .TypeInitializers
                .Where(ti => ti.Item1.TypeKind is TypeKind.Class or TypeKind.Struct)
                .ToList();
            
            foreach (var (implementation, initializer) in concreteTypes)
                initializers[implementation] = (implementation, initializer);
        }

        ImplementationToInitializer = initializers;
        
        IImmutableSet<ISymbol?> GetSetOfTypesWithProperties(
            Func<ITypesFromAttributes, IReadOnlyList<INamedTypeSymbol>> propertyGivingTypesGetter,
        Func<ITypesFromAttributes, IReadOnlyList<INamedTypeSymbol>> filteredPropertyGivingTypesGetter)
        {
            var ret = ImmutableHashSet<ISymbol?>.Empty;
            foreach (var types in typesFromAttributes)
            {
                ret = ret.Except(filteredPropertyGivingTypesGetter(types));
                
                var propertyGivingTypes = propertyGivingTypesGetter(types);
                ret = ret.Union(allImplementations
                    .Where(i =>
                    {
                        var derivedTypes = AllDerivedTypes(i).Select(t => t.OriginalDefinition).ToList();
                        return propertyGivingTypes.Any(t =>
                            derivedTypes.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                    })
                    .Distinct(SymbolEqualityComparer.Default)
                    .ToImmutableHashSet(SymbolEqualityComparer.Default));
            }
            
            return ret;
        }
        
        IEnumerable<INamedTypeSymbol> AllDerivedTypes(INamedTypeSymbol type)
        {
            var concreteTypes = new List<INamedTypeSymbol>();
            var temp = type;
            while (temp is {})
            {
                concreteTypes.Add(temp);
                temp = temp.BaseType;
            }
            return type
                .AllInterfaces
                .Append(type)
                .Concat(concreteTypes);
        }
    }
    
    public IImmutableSet<ISymbol?> TransientTypes { get; }
    public IImmutableSet<ISymbol?> ContainerInstanceTypes { get; }
    public IImmutableSet<ISymbol?> TransientScopeInstanceTypes { get; }
    public IImmutableSet<ISymbol?> ScopeInstanceTypes { get; }
    public IImmutableSet<ISymbol?> TransientScopeRootTypes { get; }
    public IImmutableSet<ISymbol?> ScopeRootTypes { get; }
    public IReadOnlyDictionary<ISymbol?, INamedTypeSymbol> InterfaceToComposite { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> ImplementationToConstructorChoice { get; }
    public IReadOnlyDictionary<ISymbol?, IReadOnlyList<INamedTypeSymbol>> InterfaceToDecorators { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> InterfaceSequenceChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationSequenceChoices { get; }
    public IReadOnlyDictionary<ITypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationMap { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
}