namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedConstrParamsInjectionMethod : IValidateUserDefinedInjectionMethod
{
    
}

internal class ValidateUserDefinedConstrParamsInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedConstrParamsInjectionMethod
{
    internal ValidateUserDefinedConstrParamsInjectionMethod(WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) => 
        InjectionAttribute = wellKnownTypesMiscellaneous.UserDefinedConstrParamsInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}