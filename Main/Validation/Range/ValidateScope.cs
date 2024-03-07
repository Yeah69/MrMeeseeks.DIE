using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range.UserDefined;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateScope : IValidateScopeBase;

internal sealed class ValidateScope : ValidateScopeBase, IValidateScope
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

    protected override string DefaultScopeName => Constants.DefaultScopeName;
    protected override string CustomScopeName => Constants.CustomScopeName;
    protected override string ScopeName => Constants.ScopeName;

    protected override DiagLogData ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification) => 
        ErrorLogData.ValidationScope(rangeType, containerType, specification);
}