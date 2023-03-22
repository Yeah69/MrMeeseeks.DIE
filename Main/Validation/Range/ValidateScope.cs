using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range.UserDefined;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateScope : IValidateScopeBase
{
}

internal class ValidateScope : ValidateScopeBase, IValidateScope
{
    internal ValidateScope(
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        IValidateAttributes validateAttributes,
        IContainerWideContext containerWideContext) 
        : base(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync,
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            validateAttributes,
            containerWideContext)
    {
        
    }

    protected override string DefaultScopeName => Constants.DefaultScopeName;
    protected override string CustomScopeName => Constants.CustomScopeName;
    protected override string ScopeName => Constants.ScopeName;

    protected override Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification) => 
        ValidationErrorDiagnostic(rangeType, containerType, specification, rangeType.Locations.FirstOrDefault() ?? Location.None);

    protected override Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification, Location location) => 
        Diagnostics.ValidationScope(rangeType, containerType, specification, location, ExecutionPhase.Validation);
}