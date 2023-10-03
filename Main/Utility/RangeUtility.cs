using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal interface IRangeUtility
{
    bool IsAContainer(INamedTypeSymbol rangeType);
    IReadOnlyList<AttributeData> GetRangeAttributes(INamedTypeSymbol rangeType);
    IReadOnlyList<ISymbol> GetUnfilteredMembers(INamedTypeSymbol rangeType);
    IReadOnlyList<ISymbol> GetEffectiveMembers(INamedTypeSymbol rangeType);
}

internal class RangeUtility : IRangeUtility
{
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;

    public RangeUtility(IContainerWideContext transientScopeWideContext)
    {
        _wellKnownTypesMiscellaneous = transientScopeWideContext.WellKnownTypesMiscellaneous;
    }

    public bool IsAContainer(INamedTypeSymbol rangeType) =>
        rangeType is { IsAbstract: false, ContainingType: null } 
        && rangeType
            .AllBaseTypesAndSelf()
            .Concat(rangeType.AllInterfaces)
            .SelectMany(t => t.GetAttributes())
            .Any(ad => CustomSymbolEqualityComparer.Default.Equals(
                _wellKnownTypesMiscellaneous.CreateFunctionAttribute,
                ad.AttributeClass));

    public IReadOnlyList<AttributeData> GetRangeAttributes(INamedTypeSymbol rangeType) =>
        rangeType
            .AllBaseTypesAndSelf()
            .Concat(rangeType.AllInterfaces)
            .SelectMany(t => t.GetAttributes())
            .ToArray();

    public IReadOnlyList<ISymbol> GetUnfilteredMembers(INamedTypeSymbol rangeType) =>
        rangeType
            .AllBaseTypesAndSelf()
            .SelectMany(t => t.GetMembers())
            .ToArray();

    public IReadOnlyList<ISymbol> GetEffectiveMembers(INamedTypeSymbol rangeType) =>
        rangeType
            .GetMembers()
            .Select(s => (s, i: 0))
            .Concat(rangeType
                .AllBaseTypes()
                .SelectMany((t, i) => t
                    .GetMembers()
                    .Where(s => s.DeclaredAccessibility == Accessibility.Protected)
                    .Select(s => (s, i: i + 1))))
            .GroupBy(t => t.s.Name)
            .Select(g => g.OrderBy(t => t.i).Select(t => t.s).First())
            .ToArray();
}