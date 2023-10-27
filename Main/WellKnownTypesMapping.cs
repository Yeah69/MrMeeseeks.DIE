using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesMapping(
    INamedTypeSymbol InjectionKeyMappingAttribute,
    INamedTypeSymbol FilterInjectionKeyMappingAttribute,
    INamedTypeSymbol DecorationOrdinalMappingAttribute,
    INamedTypeSymbol FilterDecorationOrdinalMappingAttribute)
{
    internal static WellKnownTypesMapping Create(Compilation compilation) => new (
        InjectionKeyMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(InjectionKeyMappingAttribute).FullName ?? ""),
        FilterInjectionKeyMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterInjectionKeyMappingAttribute).FullName ?? ""),
        DecorationOrdinalMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(DecorationOrdinalMappingAttribute).FullName ?? ""),
        FilterDecorationOrdinalMappingAttribute:  compilation.GetTypeByMetadataNameOrThrow(typeof(FilterDecorationOrdinalMappingAttribute).FullName ?? ""));
}