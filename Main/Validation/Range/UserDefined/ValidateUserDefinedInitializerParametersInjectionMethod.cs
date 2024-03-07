using MrMeeseeks.DIE.Logging;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedInitializerParametersInjectionMethod : IValidateUserDefinedInjectionMethod;

internal sealed class ValidateUserDefinedInitializerParametersInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedInitializerParametersInjectionMethod
{
    internal ValidateUserDefinedInitializerParametersInjectionMethod(
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        ILocalDiagLogger diagLogger) 
        : base(diagLogger) => 
        InjectionAttribute = wellKnownTypesMiscellaneous.UserDefinedInitializerParametersInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}