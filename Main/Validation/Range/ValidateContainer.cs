using MrMeeseeks.DIE.Validation.Range.UserDefined;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateContainer : IValidateRange
{
}

internal class ValidateContainer : ValidateRange, IValidateContainer
{
    private readonly IValidateTransientScope _validateTransientScopeFactory;
    private readonly IValidateScope _validateScopeFactory;

    internal ValidateContainer(
        IValidateTransientScope validateTransientScopeFactory,
        IValidateScope validateScopeFactory,
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstrParam validateUserDefinedConstrParam,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        WellKnownTypes wellKnownTypes) 
        : base(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstrParam, 
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            wellKnownTypes)
    {
        _validateTransientScopeFactory = validateTransientScopeFactory;
        _validateScopeFactory = validateScopeFactory;
    }

    public override IEnumerable<Diagnostic> Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        foreach (var diagnostic in base.Validate(rangeType, containerType))
            yield return diagnostic;

        if (rangeType.GetTypeMembers(Constants.DefaultTransientScopeName, 0).FirstOrDefault() is
            { } defaultTransientScope)
            foreach (var diagnostic in _validateTransientScopeFactory.Validate(defaultTransientScope, rangeType))
                yield return diagnostic;

        foreach (var customTransientScope in rangeType
                     .GetTypeMembers()
                     .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName)))
            foreach (var diagnostic in _validateTransientScopeFactory.Validate(customTransientScope, rangeType))
                yield return diagnostic;

        if (rangeType.GetTypeMembers(Constants.DefaultScopeName, 0).FirstOrDefault() is
            { } defaultScope)
            foreach (var diagnostic in _validateScopeFactory.Validate(defaultScope, rangeType))
                yield return diagnostic;

        foreach (var customScope in rangeType
                     .GetTypeMembers()
                     .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName)))
            foreach (var diagnostic in _validateScopeFactory.Validate(customScope, rangeType))
                yield return diagnostic;
    }

    protected override Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol _, string specification) => 
        Diagnostics.ValidationContainer(rangeType, specification);
}