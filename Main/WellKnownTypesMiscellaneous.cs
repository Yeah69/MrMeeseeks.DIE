using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesMiscellaneous(
    INamedTypeSymbol TypeInitializerAttribute,
    INamedTypeSymbol FilterTypeInitializerAttribute,
    INamedTypeSymbol CustomScopeForRootTypesAttribute,
    INamedTypeSymbol CreateFunctionAttribute,
    INamedTypeSymbol ErrorDescriptionInsteadOfBuildFailureAttribute,
    INamedTypeSymbol DieExceptionKind)
{
    internal static bool TryCreate(Compilation compilation, out WellKnownTypesMiscellaneous wellKnownTypes)
    {
        var customScopeForRootTypesAttribute = compilation
            .GetTypeByMetadataName(typeof(CustomScopeForRootTypesAttribute).FullName ?? "");

        var typeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(TypeInitializerAttribute).FullName ?? "");

        var filterTypeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTypeInitializerAttribute).FullName ?? "");

        var createFunctionAttribute = compilation
            .GetTypeByMetadataName(typeof(CreateFunctionAttribute).FullName ?? "");
        
        var errorDescriptionInsteadOfBuildFailureAttribute = compilation
            .GetTypeByMetadataName(typeof(ErrorDescriptionInsteadOfBuildFailureAttribute).FullName ?? "");
        
        var dieExceptionKind = compilation
            .GetTypeByMetadataName(typeof(DieExceptionKind).FullName ?? "");

        if (typeInitializerAttribute is not null
            && filterTypeInitializerAttribute is not null
            && customScopeForRootTypesAttribute is not null
            && createFunctionAttribute is not null
            && errorDescriptionInsteadOfBuildFailureAttribute is not null
            && dieExceptionKind is not null)
        {

            wellKnownTypes = new WellKnownTypesMiscellaneous(
                TypeInitializerAttribute: typeInitializerAttribute,
                FilterTypeInitializerAttribute: filterTypeInitializerAttribute,
                CustomScopeForRootTypesAttribute: customScopeForRootTypesAttribute,
                CreateFunctionAttribute: createFunctionAttribute,
                ErrorDescriptionInsteadOfBuildFailureAttribute: errorDescriptionInsteadOfBuildFailureAttribute,
                DieExceptionKind: dieExceptionKind);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}