namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedPropertiesMethod: IValidateUserDefinedInjectionMethod
{
    
}

internal class ValidateUserDefinedPropertiesInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedPropertiesMethod
{
    internal ValidateUserDefinedPropertiesInjectionMethod(WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) => 
        InjectionAttribute = wellKnownTypesMiscellaneous.UserDefinedPropertiesInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}