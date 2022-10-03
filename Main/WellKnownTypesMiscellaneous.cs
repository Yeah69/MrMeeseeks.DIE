using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesMiscellaneous(
    INamedTypeSymbol InitializerAttribute,
    INamedTypeSymbol FilterInitializerAttribute,
    INamedTypeSymbol CustomScopeForRootTypesAttribute,
    INamedTypeSymbol UserDefinedConstructorParametersInjectionAttribute,
    INamedTypeSymbol UserDefinedPropertiesInjectionAttribute,
    INamedTypeSymbol UserDefinedInitializerParametersInjectionAttribute,
    INamedTypeSymbol CreateFunctionAttribute,
    INamedTypeSymbol ErrorDescriptionInsteadOfBuildFailureAttribute,
    INamedTypeSymbol DieExceptionKind)
{
    internal static bool TryCreate(Compilation compilation, out WellKnownTypesMiscellaneous wellKnownTypes)
    {
        var customScopeForRootTypesAttribute = compilation
            .GetTypeByMetadataName(typeof(CustomScopeForRootTypesAttribute).FullName ?? "");

        var userDefinedConstructorParametersInjectionAttribute = compilation
            .GetTypeByMetadataName(typeof(UserDefinedConstructorParametersInjectionAttribute).FullName ?? "");

        var userDefinedInitializerParametersInjectionAttribute = compilation
            .GetTypeByMetadataName(typeof(UserDefinedInitializerParametersInjectionAttribute).FullName ?? "");

        var userDefinedPropertiesInjectionAttribute = compilation
            .GetTypeByMetadataName(typeof(UserDefinedPropertiesInjectionAttribute).FullName ?? "");

        var typeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(InitializerAttribute).FullName ?? "");

        var filterInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterInitializerAttribute).FullName ?? "");

        var createFunctionAttribute = compilation
            .GetTypeByMetadataName(typeof(CreateFunctionAttribute).FullName ?? "");
        
        var errorDescriptionInsteadOfBuildFailureAttribute = compilation
            .GetTypeByMetadataName(typeof(ErrorDescriptionInsteadOfBuildFailureAttribute).FullName ?? "");
        
        var dieExceptionKind = compilation
            .GetTypeByMetadataName(typeof(DieExceptionKind).FullName ?? "");

        if (typeInitializerAttribute is not null
            && filterInitializerAttribute is not null
            && customScopeForRootTypesAttribute is not null
            && userDefinedConstructorParametersInjectionAttribute is not null
            && userDefinedPropertiesInjectionAttribute is not null
            && userDefinedInitializerParametersInjectionAttribute is not null
            && createFunctionAttribute is not null
            && errorDescriptionInsteadOfBuildFailureAttribute is not null
            && dieExceptionKind is not null)
        {

            wellKnownTypes = new WellKnownTypesMiscellaneous(
                InitializerAttribute: typeInitializerAttribute,
                FilterInitializerAttribute: filterInitializerAttribute,
                CustomScopeForRootTypesAttribute: customScopeForRootTypesAttribute,
                UserDefinedConstructorParametersInjectionAttribute: userDefinedConstructorParametersInjectionAttribute,
                UserDefinedPropertiesInjectionAttribute: userDefinedPropertiesInjectionAttribute,
                UserDefinedInitializerParametersInjectionAttribute: userDefinedInitializerParametersInjectionAttribute, 
                CreateFunctionAttribute: createFunctionAttribute,
                ErrorDescriptionInsteadOfBuildFailureAttribute: errorDescriptionInsteadOfBuildFailureAttribute,
                DieExceptionKind: dieExceptionKind);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}