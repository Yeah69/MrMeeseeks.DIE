using MrMeeseeks.DIE.Configuration.Attributes;

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
    INamedTypeSymbol FilterImplementationCollectionChoiceAttribute,
    INamedTypeSymbol CustomConstructorParameterChoiceAttribute)
{
    internal static bool TryCreate(Compilation compilation, out WellKnownTypesChoice wellKnownTypes)
    {
        var genericParameterSubstitutesChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(GenericParameterSubstitutesChoiceAttribute).FullName ?? "");

        var filterGenericParameterSubstitutesChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterGenericParameterSubstitutesChoiceAttribute).FullName ?? "");

        var genericParameterChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(GenericParameterChoiceAttribute).FullName ?? "");

        var filterGenericParameterChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterGenericParameterChoiceAttribute).FullName ?? "");

        var decoratorSequenceChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(DecoratorSequenceChoiceAttribute).FullName ?? "");

        var constructorChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ConstructorChoiceAttribute).FullName ?? "");

        var propertyChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(PropertyChoiceAttribute).FullName ?? "");

        var implementationChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ImplementationChoiceAttribute).FullName ?? "");

        var implementationCollectionChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ImplementationCollectionChoiceAttribute).FullName ?? "");

        var filterDecoratorSequenceChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterDecoratorSequenceChoiceAttribute).FullName ?? "");

        var filterConstructorChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterConstructorChoiceAttribute).FullName ?? "");

        var filterPropertyChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterPropertyChoiceAttribute).FullName ?? "");

        var filterImplementationChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterImplementationChoiceAttribute).FullName ?? "");

        var filterImplementationCollectionChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterImplementationCollectionChoiceAttribute).FullName ?? "");

        var customConstructorParameterChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(CustomConstructorParameterChoiceAttribute).FullName ?? "");

        if (implementationChoiceAttribute is not null
            && implementationCollectionChoiceAttribute is not null
            && genericParameterSubstitutesChoiceAttribute is not null
            && genericParameterChoiceAttribute is not null
            && decoratorSequenceChoiceAttribute is not null
            && constructorChoiceAttribute is not null
            && propertyChoiceAttribute is not null
            && filterGenericParameterSubstitutesChoiceAttribute is not null
            && filterGenericParameterChoiceAttribute is not null
            && filterDecoratorSequenceChoiceAttribute is not null
            && filterConstructorChoiceAttribute is not null
            && filterPropertyChoiceAttribute is not null
            && filterImplementationChoiceAttribute is not null
            && filterImplementationCollectionChoiceAttribute is not null
            && customConstructorParameterChoiceAttribute is not null)
        {

            wellKnownTypes = new WellKnownTypesChoice(
                ImplementationChoiceAttribute: implementationChoiceAttribute,
                ImplementationCollectionChoiceAttribute: implementationCollectionChoiceAttribute,
                GenericParameterSubstitutesChoiceAttribute: genericParameterSubstitutesChoiceAttribute,
                GenericParameterChoiceAttribute: genericParameterChoiceAttribute,
                DecoratorSequenceChoiceAttribute: decoratorSequenceChoiceAttribute,
                ConstructorChoiceAttribute: constructorChoiceAttribute,
                PropertyChoiceAttribute: propertyChoiceAttribute,
                FilterGenericParameterSubstitutesChoiceAttribute: filterGenericParameterSubstitutesChoiceAttribute,
                FilterGenericParameterChoiceAttribute: filterGenericParameterChoiceAttribute,
                FilterDecoratorSequenceChoiceAttribute: filterDecoratorSequenceChoiceAttribute,
                FilterConstructorChoiceAttribute: filterConstructorChoiceAttribute,
                FilterPropertyChoiceAttribute: filterPropertyChoiceAttribute,
                FilterImplementationChoiceAttribute: filterImplementationChoiceAttribute,
                FilterImplementationCollectionChoiceAttribute: filterImplementationCollectionChoiceAttribute,
                CustomConstructorParameterChoiceAttribute: customConstructorParameterChoiceAttribute);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}