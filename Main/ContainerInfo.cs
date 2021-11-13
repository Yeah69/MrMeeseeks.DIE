namespace MrMeeseeks.DIE;

public interface IContainerInfo
{
    string Name { get; }
    string Namespace { get; }
    bool IsValid { get; }
    IReadOnlyList<INamedTypeSymbol> ResolutionRootTypes { get; }
}

internal class ContainerInfo : IContainerInfo
{
    public ContainerInfo(
        // parameters
        INamedTypeSymbol containerClass,
            
        // dependencies
        WellKnownTypes wellKnowTypes)
    {
        Name = containerClass.Name;
        Namespace = containerClass.ContainingNamespace.FullName();
            
        ResolutionRootTypes = containerClass
            .AllInterfaces
            .Where(x => x.OriginalDefinition.Equals(wellKnowTypes.Container, SymbolEqualityComparer.Default))
            .Select(nts =>
            {
                var typeParameterSymbol = nts.TypeArguments.Single();
                if (typeParameterSymbol is INamedTypeSymbol { IsUnboundGenericType: false } type && type.IsAccessibleInternally())
                {
                    return typeParameterSymbol;
                }
                return null;
            })
            .OfType<INamedTypeSymbol>()
            .ToList();

        IsValid = ResolutionRootTypes.Any();
    }

    public string Name { get; }
    public string Namespace { get; }
    public bool IsValid { get; }
    public IReadOnlyList<INamedTypeSymbol> ResolutionRootTypes { get; }
}