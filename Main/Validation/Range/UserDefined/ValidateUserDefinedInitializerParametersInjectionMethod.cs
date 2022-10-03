namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedInitializerParametersInjectionMethod : IValidateUserDefinedInjectionMethod
{
    
}

internal class ValidateUserDefinedInitializerParametersInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedInitializerParametersInjectionMethod
{
    internal ValidateUserDefinedInitializerParametersInjectionMethod(WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) => 
        InjectionAttribute = wellKnownTypesMiscellaneous.UserDefinedInitializerParametersInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}