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
}

internal class CurrentlyConsideredTypes : ICurrentlyConsideredTypes
{
    public CurrentlyConsideredTypes(
        IReadOnlyList<ITypesFromAttributes> typesFromAttributes,
        GeneratorExecutionContext context)
    {
        var tempAllImplementations = new List<INamedTypeSymbol>();
        
        tempAllImplementations.AddRange(context.Compilation.SyntaxTrees
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
            var spiedImplementations = types
                .Spy
                .SelectMany(t => t?.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(ms => !ms.ReturnsVoid)
                    .Select(ms => ms.ReturnType)
                    .OfType<INamedTypeSymbol>());

            tempAllImplementations.AddRange(types.Implementation
                .Concat(spiedImplementations));
        }

        var allImplementations = tempAllImplementations;
        
        TransientTypes = GetSetOfTypesWithProperties(t => t.Transient);
        ContainerInstanceTypes = GetSetOfTypesWithProperties(t => t.ContainerInstance);
        TransientScopeInstanceTypes = GetSetOfTypesWithProperties(t => t.TransientScopeInstance);
        ScopeInstanceTypes = GetSetOfTypesWithProperties(t => t.ScopeInstance);
        TransientScopeRootTypes = GetSetOfTypesWithProperties(t => t.TransientScopeRoot);
        ScopeRootTypes = GetSetOfTypesWithProperties(t => t.ScopeRoot);

        var compositeInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            compositeInterfaces = compositeInterfaces.Union(types.Composite);
        }
        
        var compositeTypes = GetSetOfTypesWithProperties(t => t.Composite);
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
            foreach (var constructorChoice in types.ConstructorChoices)
            {
                constructorChoices[constructorChoice.Item1] = constructorChoice.Item2;
            }
        }
        
        ImplementationToConstructorChoice = constructorChoices;
        
        var decoratorInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            decoratorInterfaces = decoratorInterfaces.Union(types.Decorator);
        }
        
        var decoratorTypes = GetSetOfTypesWithProperties(t => t.Decorator);
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
            foreach (var decoratorSequenceChoice in types.DecoratorSequenceChoices)
            {
                decoratorSequenceChoices[decoratorSequenceChoice.Item1] = decoratorSequenceChoice.Item2;
            }
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
        
        IImmutableSet<ISymbol?> GetSetOfTypesWithProperties(Func<ITypesFromAttributes, IReadOnlyList<INamedTypeSymbol>> propertyGivingTypesGetter)
        {
            var tempSet = ImmutableHashSet<ISymbol?>.Empty;
            foreach (var types in typesFromAttributes)
            {
                var propertyGivingTypes = propertyGivingTypesGetter(types);
                tempSet = tempSet.Union(allImplementations
                    .Where(i =>
                    {
                        var derivedTypes = AllDerivedTypes(i).Select(t => t.OriginalDefinition).ToList();
                        return propertyGivingTypes.Any(t =>
                            derivedTypes.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                    })
                    .Distinct(SymbolEqualityComparer.Default)
                    .ToImmutableHashSet(SymbolEqualityComparer.Default));
            }
            
            return tempSet;
        

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
}