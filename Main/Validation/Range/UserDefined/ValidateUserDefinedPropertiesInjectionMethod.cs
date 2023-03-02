using MrMeeseeks.DIE.Contexts;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedPropertiesMethod: IValidateUserDefinedInjectionMethod
{
    
}

internal class ValidateUserDefinedPropertiesInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedPropertiesMethod
{
    internal ValidateUserDefinedPropertiesInjectionMethod(IContainerWideContext containerWideContext) => 
        InjectionAttribute = containerWideContext.WellKnownTypesMiscellaneous.UserDefinedPropertiesInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}