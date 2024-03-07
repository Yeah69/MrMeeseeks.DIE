using MrMeeseeks.DIE.Logging;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedConstructorParametersInjectionMethod : IValidateUserDefinedInjectionMethod;

internal sealed class ValidateUserDefinedConstructorParametersInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedConstructorParametersInjectionMethod
{
    internal ValidateUserDefinedConstructorParametersInjectionMethod(
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        ILocalDiagLogger diagLogger) 
        : base(diagLogger) => 
        InjectionAttribute = wellKnownTypesMiscellaneous.UserDefinedConstructorParametersInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}