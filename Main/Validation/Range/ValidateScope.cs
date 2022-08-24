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
        IValidateUserDefinedConstrParamsInjectionMethod validateUserDefinedConstrParamsInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) 
        : base(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync,
            validateUserDefinedConstrParamsInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            wellKnownTypes,
            wellKnownTypesAggregation, 
            wellKnownTypesMiscellaneous)
    {
        
    }

    protected override string DefaultScopeName => Constants.DefaultScopeName;
    protected override string CustomScopeName => Constants.CustomScopeName;
    protected override string ScopeName => Constants.ScopeName;

    protected override Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification) => 
        Diagnostics.ValidationScope(rangeType, containerType, specification);
}