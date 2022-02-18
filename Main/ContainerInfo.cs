namespace MrMeeseeks.DIE;

internal interface IContainerInfo
{
    string Name { get; }
    string Namespace { get; }
    string FullName { get; }
    bool IsValid { get; }
    INamedTypeSymbol ContainerType { get; }
    IReadOnlyList<INamedTypeSymbol> ResolutionRootTypes { get; }
}

internal class ContainerInfo : IContainerInfo
{
    internal ContainerInfo(
        // parameters
        INamedTypeSymbol containerClass,
            
        // dependencies
        WellKnownTypes wellKnowTypes)
    {
        Name = containerClass.Name;
        Namespace = containerClass.ContainingNamespace.FullName();
        FullName = containerClass.FullName();
        ContainerType = containerClass;
            
        ResolutionRootTypes = containerClass
            .GetAttributes()
            .Where(ad => wellKnowTypes.MultiContainerAttribute.Equals(ad.AttributeClass, SymbolEqualityComparer.Default))
            .SelectMany(ad => ad.ConstructorArguments
                .Where(tc => tc.Kind == TypedConstantKind.Type)
                .OfType<TypedConstant>()
                .Concat(ad.ConstructorArguments.SelectMany(ca => ca.Kind is TypedConstantKind.Array
                    ? (IEnumerable<TypedConstant>)ca.Values
                    : Array.Empty<TypedConstant>())))
            .Select(tc => tc.Value as INamedTypeSymbol)
            .OfType<INamedTypeSymbol>()
            .ToList();

        IsValid = ResolutionRootTypes.Any();
    }

    public string Name { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public bool IsValid { get; }
    public INamedTypeSymbol ContainerType { get; }
    public IReadOnlyList<INamedTypeSymbol> ResolutionRootTypes { get; }
}