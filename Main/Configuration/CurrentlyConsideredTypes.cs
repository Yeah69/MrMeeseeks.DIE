using Microsoft.CodeAnalysis.CSharp.Syntax;
using MrMeeseeks.DIE.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal interface ICurrentlyConsideredTypes
{
    IImmutableSet<ISymbol?> SyncTransientTypes { get; }
    IImmutableSet<ISymbol?> AsyncTransientTypes { get; }
    IImmutableSet<ISymbol?> ContainerInstanceTypes { get; }
    IImmutableSet<ISymbol?> TransientScopeInstanceTypes { get; }
    IImmutableSet<ISymbol?> ScopeInstanceTypes { get; }
    IImmutableSet<ISymbol?> TransientScopeRootTypes { get; }
    IImmutableSet<ISymbol?> ScopeRootTypes { get; }
    IReadOnlyDictionary<ISymbol?, INamedTypeSymbol> InterfaceToComposite { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> ImplementationToConstructorChoice { get; }
    IReadOnlyDictionary<ISymbol?, IReadOnlyList<INamedTypeSymbol>> InterfaceToDecorators { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> InterfaceSequenceChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationSequenceChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationMap { get; }
    IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutes { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; } 
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
                .OfType<INamedTypeSymbol>()
                .Where(nts => !nts.IsAbstract)));
        
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
                    .OfType<INamedTypeSymbol>()
                    .Where(nts => !nts.IsAbstract));

            allImplementations.AddRange(types.Implementation
                .Concat(spiedImplementations));
        }

        SyncTransientTypes = GetSetOfTypesWithProperties(
            t => t.SyncTransient.Concat(t.Transient).ToList(), 
            t => t.FilterSyncTransient.Concat(t.FilterTransient).ToList());
        AsyncTransientTypes = GetSetOfTypesWithProperties(
            t => t.AsyncTransient.Concat(t.Transient).ToList(), 
            t => t.FilterAsyncTransient.Concat(t.FilterTransient).ToList());
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
            .SelectMany(i => { return i.AllInterfaces.OfType<INamedTypeSymbol>().Select(ii => (ii, i)).Prepend((i, i)); })
            .GroupBy(t => t.Item1.UnboundIfGeneric(), t => t.Item2)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<INamedTypeSymbol>) g.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToList());

        var initializers = new Dictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)>(SymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            var filterInterfaceTypes = types
                .FilterTypeInitializers
                .Where(t => t.TypeKind is TypeKind.Interface)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
            
            foreach (var filterConcreteType in allImplementations
                         .Where(i => i.AllDerivedTypes()
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
                                 if (i.AllDerivedTypes().Select(d => d.OriginalDefinition).Contains(interfaceType, SymbolEqualityComparer.Default))
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

        var genericParameterSubstitutes =
            new Dictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>>();
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var tuple in typesFromAttribute.FilterGenericParameterSubstitutes)
            {
                var key = (tuple.Item1, tuple.Item2);
                if (genericParameterSubstitutes.TryGetValue(key, out var currentGenericTypes))
                {
                    var newGenericTypes = currentGenericTypes
                        .Where(gt => !tuple.Item3.Contains(gt, SymbolEqualityComparer.Default)).ToImmutableList();
                    if (newGenericTypes.Any())
                        genericParameterSubstitutes[key] = newGenericTypes;
                    else
                        genericParameterSubstitutes.Remove(key);
                }
            }

            foreach (var tuple in typesFromAttribute.GenericParameterSubstitutes)
            {
                var key = (tuple.Item1, tuple.Item2);
                var list = new List<INamedTypeSymbol>(
                    genericParameterSubstitutes.TryGetValue(key, out var currentGenericTypes)
                        ? currentGenericTypes
                        : Array.Empty<INamedTypeSymbol>());
                foreach (var newGenericType in tuple.Item3)
                {
                    if (!list.Contains(newGenericType, SymbolEqualityComparer.Default))
                        list.Add(newGenericType);
                }

                genericParameterSubstitutes[key] = list;
            }
        }

        GenericParameterSubstitutes = genericParameterSubstitutes;

        var genericParameterChoices =
            new Dictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol>();
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var tuple in typesFromAttribute.FilterGenericParameterChoices)
                genericParameterChoices.Remove((tuple.Item1, tuple.Item2));

            foreach (var tuple in typesFromAttribute.GenericParameterChoices)
                genericParameterChoices[(tuple.Item1, tuple.Item2)] = tuple.Item3;
        }

        GenericParameterChoices = genericParameterChoices;
        
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
                        var derivedTypes = i.AllDerivedTypes().Select(t => t.OriginalDefinition).ToList();
                        return propertyGivingTypes.Any(t =>
                            derivedTypes.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                    })
                    .Distinct(SymbolEqualityComparer.Default)
                    .ToImmutableHashSet(SymbolEqualityComparer.Default));
            }
            
            return ret;
        }
    }
    
    public IImmutableSet<ISymbol?> SyncTransientTypes { get; }
    public IImmutableSet<ISymbol?> AsyncTransientTypes { get; }
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
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationMap { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutes { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; }
}