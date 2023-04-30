using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesChoice(
    INamedTypeSymbol GenericParameterSubstitutesChoiceAttribute,
    INamedTypeSymbol GenericParameterChoiceAttribute,
    INamedTypeSymbol DecoratorSequenceChoiceAttribute,
    INamedTypeSymbol ConstructorChoiceAttribute,
    INamedTypeSymbol PropertyChoiceAttribute,
    INamedTypeSymbol ImplementationChoiceAttribute,
    INamedTypeSymbol ImplementationCollectionChoiceAttribute,
    INamedTypeSymbol FilterGenericParameterSubstitutesChoiceAttribute,
    INamedTypeSymbol FilterGenericParameterChoiceAttribute,
    INamedTypeSymbol FilterDecoratorSequenceChoiceAttribute,
    INamedTypeSymbol FilterConstructorChoiceAttribute,
    INamedTypeSymbol FilterPropertyChoiceAttribute,
    INamedTypeSymbol FilterImplementationChoiceAttribute,
    INamedTypeSymbol FilterImplementationCollectionChoiceAttribute)
{
    internal static WellKnownTypesChoice Create(Compilation compilation) => new (
        ImplementationChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(ImplementationChoiceAttribute).FullName ?? ""),
        ImplementationCollectionChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(ImplementationCollectionChoiceAttribute).FullName ?? ""),
        GenericParameterSubstitutesChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(GenericParameterSubstitutesChoiceAttribute).FullName ?? ""),
        GenericParameterChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(GenericParameterChoiceAttribute).FullName ?? ""),
        DecoratorSequenceChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(DecoratorSequenceChoiceAttribute).FullName ?? ""),
        ConstructorChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(ConstructorChoiceAttribute).FullName ?? ""),
        PropertyChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(PropertyChoiceAttribute).FullName ?? ""),
        FilterGenericParameterSubstitutesChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterGenericParameterSubstitutesChoiceAttribute).FullName ?? ""),
        FilterGenericParameterChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterGenericParameterChoiceAttribute).FullName ?? ""),
        FilterDecoratorSequenceChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterDecoratorSequenceChoiceAttribute).FullName ?? ""),
        FilterConstructorChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterConstructorChoiceAttribute).FullName ?? ""),
        FilterPropertyChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterPropertyChoiceAttribute).FullName ?? ""),
        FilterImplementationChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterImplementationChoiceAttribute).FullName ?? ""),
        FilterImplementationCollectionChoiceAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterImplementationCollectionChoiceAttribute).FullName ?? ""));
}