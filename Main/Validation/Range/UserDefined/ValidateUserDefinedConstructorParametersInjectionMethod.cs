namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedConstructorParametersInjectionMethod : IValidateUserDefinedInjectionMethod
{
    
}

internal class ValidateUserDefinedConstructorParametersInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedConstructorParametersInjectionMethod
{
    internal ValidateUserDefinedConstructorParametersInjectionMethod(WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) => 
        InjectionAttribute = wellKnownTypesMiscellaneous.UserDefinedConstructorParametersInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}