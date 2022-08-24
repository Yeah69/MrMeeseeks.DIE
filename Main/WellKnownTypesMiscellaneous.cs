using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesMiscellaneous(
    INamedTypeSymbol TypeInitializerAttribute,
    INamedTypeSymbol FilterTypeInitializerAttribute,
    INamedTypeSymbol CustomScopeForRootTypesAttribute,
    INamedTypeSymbol UserDefinedConstrParamsInjectionAttribute,
    INamedTypeSymbol UserDefinedPropertiesInjectionAttribute,
    INamedTypeSymbol CreateFunctionAttribute,
    INamedTypeSymbol ErrorDescriptionInsteadOfBuildFailureAttribute,
    INamedTypeSymbol DieExceptionKind)
{
    internal static bool TryCreate(Compilation compilation, out WellKnownTypesMiscellaneous wellKnownTypes)
    {
        var customScopeForRootTypesAttribute = compilation
            .GetTypeByMetadataName(typeof(CustomScopeForRootTypesAttribute).FullName ?? "");

        var userDefinedConstrParamsInjectionAttribute = compilation
            .GetTypeByMetadataName(typeof(UserDefinedConstrParamsInjectionAttribute).FullName ?? "");

        var userDefinedPropertiesInjectionAttribute = compilation
            .GetTypeByMetadataName(typeof(UserDefinedPropertiesInjectionAttribute).FullName ?? "");

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
            && userDefinedConstrParamsInjectionAttribute is not null
            && userDefinedPropertiesInjectionAttribute is not null
            && createFunctionAttribute is not null
            && errorDescriptionInsteadOfBuildFailureAttribute is not null
            && dieExceptionKind is not null)
        {

            wellKnownTypes = new WellKnownTypesMiscellaneous(
                TypeInitializerAttribute: typeInitializerAttribute,
                FilterTypeInitializerAttribute: filterTypeInitializerAttribute,
                CustomScopeForRootTypesAttribute: customScopeForRootTypesAttribute,
                UserDefinedConstrParamsInjectionAttribute: userDefinedConstrParamsInjectionAttribute,
                UserDefinedPropertiesInjectionAttribute: userDefinedPropertiesInjectionAttribute,
                CreateFunctionAttribute: createFunctionAttribute,
                ErrorDescriptionInsteadOfBuildFailureAttribute: errorDescriptionInsteadOfBuildFailureAttribute,
                DieExceptionKind: dieExceptionKind);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}