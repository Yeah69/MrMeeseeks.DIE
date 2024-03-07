using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range.UserDefined;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateTransientScope : IValidateScopeBase;

internal sealed class ValidateTransientScope : ValidateScopeBase, IValidateTransientScope
{
    internal ValidateTransientScope(
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        IValidateAttributes validateAttributes,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        ILocalDiagLogger localDiagLogger,
        IRangeUtility rangeUtility) 
        : base(
            validateUserDefinedAddForDisposalSync,
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            validateAttributes,
            wellKnownTypes,
            wellKnownTypesAggregation,
            wellKnownTypesMiscellaneous,
            localDiagLogger,
            rangeUtility)
    {
        
    }

    protected override string DefaultScopeName => Constants.DefaultTransientScopeName;
    protected override string CustomScopeName => Constants.CustomTransientScopeName;
    protected override string ScopeName => Constants.TransientScopeName;

    protected override DiagLogData ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification) => 
        ErrorLogData.ValidationTransientScope(rangeType, containerType, specification);
}