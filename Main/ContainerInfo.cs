namespace MrMeeseeks.DIE;

internal interface IContainerInfo
{
    string Name { get; }
    string Namespace { get; }
    string FullName { get; }
    INamedTypeSymbol ContainerType { get; }
    IReadOnlyList<(INamedTypeSymbol, string)> CreateFunctionData { get; }
}

internal class ContainerInfo : IContainerInfo
{
    internal ContainerInfo(
        // parameters
        INamedTypeSymbol containerClass,
            
        // dependencies
        WellKnownTypesMiscellaneous wellKnowTypesMiscellaneous)
    {
        Name = containerClass.Name;
        Namespace = containerClass.ContainingNamespace.FullName();
        FullName = containerClass.FullName();
        ContainerType = containerClass;
            
        CreateFunctionData = containerClass
            .GetAttributes()
            .Where(ad => wellKnowTypesMiscellaneous.CreateFunctionAttribute.Equals(ad.AttributeClass, SymbolEqualityComparer.Default))
            .Select(ad => ad.ConstructorArguments.Length == 2 
                          && ad.ConstructorArguments[0].Kind == TypedConstantKind.Type
                          && ad.ConstructorArguments[0].Value is INamedTypeSymbol type
                          && ad.ConstructorArguments[1].Kind == TypedConstantKind.Primitive
                          && ad.ConstructorArguments[1].Value is string methodNamePrefix
                          ? (type, methodNamePrefix)
                          : ((INamedTypeSymbol, string)?) null)
            .OfType<(INamedTypeSymbol, string)>()
            .ToList();
    }

    public string Name { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public INamedTypeSymbol ContainerType { get; }
    public IReadOnlyList<(INamedTypeSymbol, string)> CreateFunctionData { get; }
}