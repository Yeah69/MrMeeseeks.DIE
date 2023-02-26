using MrMeeseeks.DIE.Validation.Range.UserDefined;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateTransientScope : IValidateScopeBase
{
}

internal class ValidateTransientScope : ValidateScopeBase, IValidateTransientScope
{
    internal ValidateTransientScope(
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        IContainerWideContext containerWideContext) 
        : base(
            validateUserDefinedAddForDisposalSync,
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            containerWideContext)
    {
        
    }

    protected override string DefaultScopeName => Constants.DefaultTransientScopeName;
    protected override string CustomScopeName => Constants.CustomTransientScopeName;
    protected override string ScopeName => Constants.TransientScopeName;

    protected override Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification) => 
        Diagnostics.ValidationTransientScope(rangeType, containerType, specification, ExecutionPhase.Validation);
}