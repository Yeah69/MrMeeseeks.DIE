using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateContainer : IValidateRange
{
}

internal class ValidateContainer : ValidateRange, IValidateContainer
{
    private readonly IValidateTransientScope _validateTransientScopeFactory;
    private readonly IValidateScope _validateScopeFactory;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;

    internal ValidateContainer(
        IValidateTransientScope validateTransientScopeFactory,
        IValidateScope validateScopeFactory,
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        IContainerWideContext containerWideContext) 
        : base(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            containerWideContext)
    {
        _validateTransientScopeFactory = validateTransientScopeFactory;
        _validateScopeFactory = validateScopeFactory;
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
    }

    public override IEnumerable<Diagnostic> Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        foreach (var diagnostic in base.Validate(rangeType, containerType))
            yield return diagnostic;

        if (rangeType.GetTypeMembers(Constants.DefaultTransientScopeName, 0).FirstOrDefault() is
            { } defaultTransientScope)
            foreach (var diagnostic in _validateTransientScopeFactory.Validate(defaultTransientScope, rangeType))
                yield return diagnostic;

        var customTransientScopeTypes = new HashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
        foreach (var customTransientScope in rangeType
                     .GetTypeMembers()
                     .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName)))
        {
            foreach (var diagnostic in ValidateCustomScope(customTransientScope, customTransientScopeTypes))
                yield return diagnostic;
            
            foreach (var diagnostic in _validateTransientScopeFactory.Validate(customTransientScope, rangeType))
                yield return diagnostic;
        }

        if (rangeType.GetTypeMembers(Constants.DefaultScopeName, 0).FirstOrDefault() is
            { } defaultScope)
            foreach (var diagnostic in _validateScopeFactory.Validate(defaultScope, rangeType))
                yield return diagnostic;

        var customScopeTypes = new HashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
        foreach (var customScope in rangeType
                     .GetTypeMembers()
                     .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName)))
        {
            foreach (var diagnostic in ValidateCustomScope(customScope, customScopeTypes))
                yield return diagnostic;

            foreach (var diagnostic in _validateScopeFactory.Validate(customScope, rangeType))
                yield return diagnostic;
        }

        var createFunctionAttributes = rangeType
            .GetAttributes()
            .Where(ad =>
                CustomSymbolEqualityComparer.Default.Equals(
                    ad.AttributeClass, 
                    _wellKnownTypesMiscellaneous.CreateFunctionAttribute))
            .ToImmutableArray();
        
        if (!createFunctionAttributes.Any())
            yield return ValidationErrorDiagnostic(rangeType, containerType, $"The container has to have at least one attribute of type \"{nameof(CreateFunctionAttribute)}\".");

        var takenMemberNames = new HashSet<string>();
        takenMemberNames.UnionWith(rangeType.MemberNames);

        foreach (var createFunctionAttribute in createFunctionAttributes)
        {
            var location = createFunctionAttribute.GetLocation();
            if (createFunctionAttribute.ConstructorArguments.Length == 3
                && createFunctionAttribute.ConstructorArguments[1].Value is string functionName)
            {
                if (functionName == nameof(IDisposable.Dispose))
                    yield return ValidationErrorDiagnostic(rangeType, $"Create function isn't allowed to have the name \"{nameof(IDisposable.Dispose)}\", because a method with that name may have to be generated by the container.", location);
                if (functionName == nameof(IAsyncDisposable.DisposeAsync))
                    yield return ValidationErrorDiagnostic(rangeType, $"Create function isn't allowed to have the name \"{nameof(IAsyncDisposable.DisposeAsync)}\", because a method with that name will be generated by the container.", location);

                foreach (var concreteFunctionName in new [] { $"{functionName}{Constants.CreateFunctionSuffix}", $"{functionName}{Constants.CreateFunctionSuffixAsync}", $"{functionName}{Constants.CreateFunctionSuffixValueAsync}"})
                {
                    if (takenMemberNames.Contains(concreteFunctionName))
                        yield return ValidationErrorDiagnostic(rangeType, $"Create function's name \"{concreteFunctionName}\" collides with one of the other members of the container class.", location);
                    takenMemberNames.Add(concreteFunctionName);
                }
            }
            else
                yield return ValidationErrorDiagnostic(rangeType, "Attribute doesn't have expected constructor arguments.", location);
        }

        IEnumerable<Diagnostic> ValidateCustomScope(INamedTypeSymbol customScope, ISet<INamedTypeSymbol> customScopeTypesSet)
        {
            var customScopeAttributes = customScope
                .GetAttributes()
                .Where(ad => CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                    _wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
                .ToArray();

            if (customScopeAttributes.Length == 0)
            {
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"Custom scope \"{customScope.Name}\" has to have at least one \"{_wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute.FullName()}\"-attribute.");
            }

            var scopeRootTypes = customScopeAttributes
                .SelectMany(ad => ad.ConstructorArguments[0].Values)
                .Select(tc => tc.Value)
                .OfType<INamedTypeSymbol>()
                .ToList();
            foreach (var scopeRootType in scopeRootTypes
                         .Where(scopeRootType => customScopeTypesSet.Contains(scopeRootType, CustomSymbolEqualityComparer.Default)))
            {
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"Custom scope \"{customScope.Name}\" gets the type \"{scopeRootType.FullName()}\" passed into one of its \"{_wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute.FullName()}\"-attributes, but it is already in use in another custom scope.");
            }
            customScopeTypesSet.UnionWith(scopeRootTypes);
        }
    }

    protected override Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol _, string specification) => 
        Diagnostics.ValidationContainer(rangeType, specification, ExecutionPhase.Validation);
    
    private Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, string specification, Location location) => 
        Diagnostics.ValidationContainer(rangeType, specification, location, ExecutionPhase.Validation);
}