using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedInitializerParametersInjectionMethod : IValidateUserDefinedInjectionMethod;

internal sealed class ValidateUserDefinedInitializerParametersInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedInitializerParametersInjectionMethod
{
    internal ValidateUserDefinedInitializerParametersInjectionMethod(
        IContainerWideContext containerWideContext,
        ILocalDiagLogger diagLogger) 
        : base(diagLogger) => 
        InjectionAttribute = containerWideContext.WellKnownTypesMiscellaneous.UserDefinedInitializerParametersInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}