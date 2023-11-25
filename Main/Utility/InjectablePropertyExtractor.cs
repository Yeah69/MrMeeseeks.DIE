using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal interface IInjectablePropertyExtractor
{
    IEnumerable<IPropertySymbol> GetInjectableProperties(INamedTypeSymbol implementationType);
}

internal class InjectablePropertyExtractor(ICheckInternalsVisible checkInternalsVisible) : IInjectablePropertyExtractor,
    IContainerInstance
{
    public IEnumerable<IPropertySymbol> GetInjectableProperties(INamedTypeSymbol implementationType) =>
        implementationType
            .AllBaseTypesAndSelf()
            .Select((nts, i) => (nts, i))
            .SelectMany(t => t.nts.GetMembers().OfType<IPropertySymbol>().Select(p => (p, t.i)))
            .GroupBy(t => t.p.Name)
            .Select(g => g.OrderBy(t => t.i).Select(t => t.p).First())
            // Check whether property is accessible
            .Where(p => p.SetMethod is { DeclaredAccessibility: Accessibility.Public }
                        || p.SetMethod is { DeclaredAccessibility: Accessibility.Internal } &&
                        checkInternalsVisible.Check(p.SetMethod));
}