using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedConstructorParametersInjectionMethod : IValidateUserDefinedInjectionMethod
{
    
}

internal sealed class ValidateUserDefinedConstructorParametersInjectionMethod : ValidateUserDefinedInjectionMethod, IValidateUserDefinedConstructorParametersInjectionMethod
{
    internal ValidateUserDefinedConstructorParametersInjectionMethod(
        IContainerWideContext containerWideContext,
        ILocalDiagLogger diagLogger) 
        : base(diagLogger) => 
        InjectionAttribute = containerWideContext.WellKnownTypesMiscellaneous.UserDefinedConstructorParametersInjectionAttribute;

    protected override INamedTypeSymbol InjectionAttribute { get; }
}