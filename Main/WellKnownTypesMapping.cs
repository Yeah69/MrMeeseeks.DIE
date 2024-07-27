using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal sealed record WellKnownTypesMapping(
    INamedTypeSymbol InjectionKeyMappingAttribute,
    INamedTypeSymbol FilterInjectionKeyMappingAttribute,
    INamedTypeSymbol DecorationOrdinalMappingAttribute,
    INamedTypeSymbol FilterDecorationOrdinalMappingAttribute,
    INamedTypeSymbol InvocationDescriptionMappingAttribute,
    INamedTypeSymbol TypeDescriptionMappingAttribute,
    INamedTypeSymbol MethodDescriptionMappingAttribute)
    : IContainerInstance
{
    internal static WellKnownTypesMapping Create(Compilation compilation) => new (
        InjectionKeyMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(InjectionKeyMappingAttribute).FullName ?? ""),
        FilterInjectionKeyMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterInjectionKeyMappingAttribute).FullName ?? ""),
        DecorationOrdinalMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(DecorationOrdinalMappingAttribute).FullName ?? ""),
        FilterDecorationOrdinalMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterDecorationOrdinalMappingAttribute).FullName ?? ""),
        InvocationDescriptionMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(InvocationDescriptionMappingAttribute).FullName ?? ""),
        TypeDescriptionMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(TypeDescriptionMappingAttribute).FullName ?? ""),
        MethodDescriptionMappingAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(MethodDescriptionMappingAttribute).FullName ?? ""));
}