using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IContainerInfo
{
    string Name { get; }
    string Namespace { get; }
    string FullName { get; }
    INamedTypeSymbol ContainerType { get; }
    IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)> CreateFunctionData { get; }
}

internal sealed class ContainerInfo : IContainerInfo, IContainerLevelOnlyContainerInstance
{
    internal ContainerInfo(
        // parameters
        INamedTypeSymbol containerClass,
            
        // dependencies
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        IRangeUtility rangeUtility)
    {
        Name = containerClass.Name;
        Namespace = containerClass.ContainingNamespace.FullName();
        FullName = containerClass.FullName();
        ContainerType = containerClass;

        CreateFunctionData = rangeUtility.GetRangeAttributes(containerClass)
            .Where(ad => CustomSymbolEqualityComparer.Default.Equals(wellKnownTypesMiscellaneous.CreateFunctionAttribute, ad.AttributeClass))
            .Select(ad => ad.ConstructorArguments is [
                              { Kind: TypedConstantKind.Type, Value: ITypeSymbol type },
                              { Kind: TypedConstantKind.Primitive, Value: string methodNamePrefix }, 
                              { Kind: TypedConstantKind.Array }]
                          && ad.ConstructorArguments[2].Values.Select(v => v.Value).All(v => v is ITypeSymbol)
                ? (
                    type, 
                    methodNamePrefix,
                    ad.ConstructorArguments[2].Values.Select(v => v.Value).OfType<ITypeSymbol>().ToList(),
                    ad.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None)
                : ((ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)?) null)
            .OfType<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)>()
            .ToList();
    }

    public string Name { get; }
    public string Namespace { get; }
    public string FullName { get; }
    public INamedTypeSymbol ContainerType { get; }
    public IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, Location)> CreateFunctionData { get; }
}