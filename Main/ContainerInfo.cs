using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IContainerInfo
{
    string Name { get; }
    string Namespace { get; }
    string FullName { get; }
    INamedTypeSymbol ContainerType { get; }
    IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>)> CreateFunctionData { get; }
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
            .Where(ad => SymbolEqualityComparer.Default.Equals(wellKnowTypesMiscellaneous.CreateFunctionAttribute, ad.AttributeClass))
            .Select(ad => ad.ConstructorArguments.Length == 3 
                          && ad.ConstructorArguments[0].Kind == TypedConstantKind.Type
                          && ad.ConstructorArguments[0].Value is ITypeSymbol type
                          && ad.ConstructorArguments[1].Kind == TypedConstantKind.Primitive
                          && ad.ConstructorArguments[1].Value is string methodNamePrefix
                          && ad.ConstructorArguments[2].Kind == TypedConstantKind.Array
                          && ad.ConstructorArguments[2].Values.Select(v => v.Value).All(v => v is ITypeSymbol)
                          ? (type, methodNamePrefix, ad.ConstructorArguments[2].Values.Select(v => v.Value).OfType<ITypeSymbol>().ToList())
                          : ((ITypeSymbol, string, IReadOnlyList<ITypeSymbol>)?) null)
            .OfType<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>)>()
            .ToList();
    }

    public string Name { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public INamedTypeSymbol ContainerType { get; }
    public IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>)> CreateFunctionData { get; }
}