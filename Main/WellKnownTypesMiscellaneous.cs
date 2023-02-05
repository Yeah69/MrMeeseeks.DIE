using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Extensions;

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
    internal static WellKnownTypesMiscellaneous Create(Compilation compilation) => new (
        InitializerAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(InitializerAttribute).FullName ?? ""),
        FilterInitializerAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(FilterInitializerAttribute).FullName ?? ""),
        CustomScopeForRootTypesAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(CustomScopeForRootTypesAttribute).FullName ?? ""),
        UserDefinedConstructorParametersInjectionAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(UserDefinedConstructorParametersInjectionAttribute).FullName ?? ""),
        UserDefinedPropertiesInjectionAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(UserDefinedPropertiesInjectionAttribute).FullName ?? ""),
        UserDefinedInitializerParametersInjectionAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(UserDefinedInitializerParametersInjectionAttribute).FullName ?? ""), 
        CreateFunctionAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(CreateFunctionAttribute).FullName ?? ""),
        ErrorDescriptionInsteadOfBuildFailureAttribute: compilation.GetTypeByMetadataNameOrThrow(typeof(ErrorDescriptionInsteadOfBuildFailureAttribute).FullName ?? ""),
        DieExceptionKind: compilation.GetTypeByMetadataNameOrThrow(typeof(DieExceptionKind).FullName ?? ""));
}