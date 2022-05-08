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
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> DecoratorInterfaceSequenceChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> DecoratorImplementationSequenceChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationMap { get; }
    IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutes { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; } 
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<IPropertySymbol>> PropertyChoices { get; }
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

        var immutableHashSet = context
            .Compilation
            .SourceModule
            .ReferencedAssemblySymbols
            .SelectMany(a => GetAllNamespaces(a.GlobalNamespace))
            .SelectMany(ns => ns.GetTypeMembers())
            .Where(nts => nts is
            {
                IsAbstract: false, 
                IsStatic: false,
                IsImplicitClass: false,
                IsScriptClass: false,
                TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Structure,
                DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
            })
            .Where(nts => !nts.Name.StartsWith("<") && nts.IsAccessibleInternally())
            .ToImmutableHashSet(SymbolEqualityComparer.Default);

        IEnumerable<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol root)
        {
            yield return root;
            foreach(var child in root.GetNamespaceMembers())
                foreach(var next in GetAllNamespaces(child))
                    yield return next;
        }

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
            compositeInterfaces = compositeInterfaces.Except(types.FilterComposite.Select(c => c.UnboundIfGeneric()));
            compositeInterfaces = compositeInterfaces.Union(types.Composite.Select(c => c.UnboundIfGeneric()));
        }
        
        var compositeTypes = GetSetOfTypesWithProperties(t => t.Composite, t => t.FilterComposite);
        InterfaceToComposite = compositeTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.OriginalDefinition.AllInterfaces
                    .Single(t => compositeInterfaces.Contains(t.UnboundIfGeneric(), SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.FirstOrDefault() is INamedTypeSymbol interfaceTypeSymbol
                    ? interfaceTypeSymbol.UnboundIfGeneric()
                    : throw new Exception("Composite should implement composite interface");
            }, SymbolEqualityComparer.Default)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single(), SymbolEqualityComparer.Default);

        var constructorChoices = new Dictionary<INamedTypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterConstructorChoice in types.FilterConstructorChoices)
                constructorChoices.Remove(filterConstructorChoice);

            foreach (var (implementationType, constructor) in types.ConstructorChoices)
                constructorChoices[implementationType.UnboundIfGeneric()] = constructor;
        }
        
        ImplementationToConstructorChoice = constructorChoices;

        var propertyChoices = new Dictionary<INamedTypeSymbol, IReadOnlyList<IPropertySymbol>>(SymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterPropertyChoice in types.FilterPropertyChoices)
                propertyChoices.Remove(filterPropertyChoice);

            foreach (var (implementationType, properties) in types.PropertyChoices)
                propertyChoices[implementationType.UnboundIfGeneric()] = properties;
        }
        
        PropertyChoices = propertyChoices;

        var decoratorInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            decoratorInterfaces = decoratorInterfaces.Except(types.FilterDecorator.Select(c => c.UnboundIfGeneric()));
            decoratorInterfaces = decoratorInterfaces.Union(types.Decorator.Select(c => c.UnboundIfGeneric()));
        }
        
        var decoratorTypes = GetSetOfTypesWithProperties(
            t => t.Decorator, 
            t => t.FilterDecorator);
        InterfaceToDecorators = decoratorTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.OriginalDefinition.AllInterfaces
                    .Single(t => decoratorInterfaces.Contains(t.UnboundIfGeneric(), SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.FirstOrDefault() is INamedTypeSymbol interfaceTypeSymbol
                    ? interfaceTypeSymbol.UnboundIfGeneric()
                    : throw new Exception("Decorator should implement decorator interface");
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

        DecoratorInterfaceSequenceChoices = decoratorSequenceChoices
            .Where(kvp => kvp.Key.TypeKind == TypeKind.Interface)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        DecoratorImplementationSequenceChoices = decoratorSequenceChoices
            .Where(kvp => kvp.Key.TypeKind is TypeKind.Class or TypeKind.Struct)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        ImplementationMap = allImplementations
            .Where(t => !decoratorTypes.Contains(t.UnboundIfGeneric()))
            .Where(t => !compositeTypes.Contains(t.UnboundIfGeneric()))
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
                         .Where(i => i.OriginalDefinitionIfUnbound().AllDerivedTypes()
                             .Any(inter => filterInterfaceTypes.Contains(inter))))
                initializers.Remove(filterConcreteType);

            var filterConcreteTypes = types
                .FilterTypeInitializers
                .Where(t => t.TypeKind is TypeKind.Class or TypeKind.Struct)
                .ToList();
            
            foreach (var filterConcreteType in filterConcreteTypes)
                initializers.Remove(filterConcreteType.UnboundIfGeneric());
            
            var interfaceTypes = types
                .TypeInitializers
                .Where(ti => ti.Item1.TypeKind is TypeKind.Interface)
                .ToList();

            foreach (var (implementationType, interfaceType, initializerMethod) in allImplementations
                         .Select(i =>
                         {
                             foreach (var (interfaceType, initializer) in interfaceTypes)
                             {
                                 if (i.OriginalDefinitionIfUnbound().AllDerivedTypes().Contains(interfaceType, SymbolEqualityComparer.Default))
                                 {
                                     return ((INamedTypeSymbol, INamedTypeSymbol, IMethodSymbol)?) (i, interfaceType, initializer);
                                 }
                             }

                             return null;
                         })
                         .OfType<(INamedTypeSymbol, INamedTypeSymbol, IMethodSymbol)>())
                initializers[implementationType.UnboundIfGeneric()] = (interfaceType, initializerMethod);

            var concreteTypes = types
                .TypeInitializers
                .Where(ti => ti.Item1.TypeKind is TypeKind.Class or TypeKind.Struct)
                .ToList();
            
            foreach (var (implementation, initializer) in concreteTypes)
                initializers[implementation.UnboundIfGeneric()] = (implementation.UnboundIfGeneric(), initializer);
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
                var filterPropertyGivingTypes = filteredPropertyGivingTypesGetter(types);
                ret = ret.Except(allImplementations
                    .Where(i =>
                    {
                        var derivedTypes = i.AllDerivedTypes().Select(t => t.UnboundIfGeneric()).ToList();
                        return filterPropertyGivingTypes.Any(t =>
                            derivedTypes.Contains(t.UnboundIfGeneric(), SymbolEqualityComparer.Default));
                    })
                    .Select(i => i.UnboundIfGeneric())
                    .ToImmutableHashSet(SymbolEqualityComparer.Default));
                
                var propertyGivingTypes = propertyGivingTypesGetter(types);
                ret = ret.Union(allImplementations
                    .Where(i =>
                    {
                        var derivedTypes = i.AllDerivedTypes().Select(t => t.UnboundIfGeneric()).ToList();
                        return propertyGivingTypes.Any(t =>
                            derivedTypes.Contains(t.UnboundIfGeneric(), SymbolEqualityComparer.Default));
                    })
                    .Select(i => i.UnboundIfGeneric())
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
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> DecoratorInterfaceSequenceChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> DecoratorImplementationSequenceChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationMap { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutes { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<IPropertySymbol>> PropertyChoices { get; }
}