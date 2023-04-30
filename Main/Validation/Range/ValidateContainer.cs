using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Validation.Attributes;
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
        IValidateAttributes validateAttributes,
        IContainerWideContext containerWideContext,
        ILocalDiagLogger localDiagLogger) 
        : base(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync, 
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            validateAttributes,
            containerWideContext,
            localDiagLogger)
    {
        _validateTransientScopeFactory = validateTransientScopeFactory;
        _validateScopeFactory = validateScopeFactory;
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
    }

    public override void Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        base.Validate(rangeType, containerType);
        
        if (!rangeType.InstanceConstructors.Any())
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(
                    rangeType,
                    rangeType,
                    "The container class has to have at least one constructor."), 
                rangeType.Locations.FirstOrDefault() ?? Location.None);
            
        
        foreach (var instanceConstructor in rangeType
                     .InstanceConstructors
                     .Where(instanceConstructor => instanceConstructor.DeclaredAccessibility != Accessibility.Private))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(
                    rangeType, 
                    rangeType,
                    "The constructor of a container class has to be declared private."), 
                instanceConstructor.Locations.FirstOrDefault() ?? Location.None);

        if (rangeType.GetTypeMembers(Constants.DefaultTransientScopeName, 0).FirstOrDefault() is
            { } defaultTransientScope)
            _validateTransientScopeFactory.Validate(defaultTransientScope, rangeType);

        var customTransientScopeTypes = new HashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
        foreach (var customTransientScope in rangeType
                     .GetTypeMembers()
                     .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName)))
        {
            ValidateCustomScope(customTransientScope, customTransientScopeTypes);
            _validateTransientScopeFactory.Validate(customTransientScope, rangeType);
        }

        if (rangeType.GetTypeMembers(Constants.DefaultScopeName, 0).FirstOrDefault() is
            { } defaultScope)
            _validateScopeFactory.Validate(defaultScope, rangeType);

        var customScopeTypes = new HashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
        foreach (var customScope in rangeType
                     .GetTypeMembers()
                     .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName)))
        {
            ValidateCustomScope(customScope, customScopeTypes);
            _validateScopeFactory.Validate(customScope, rangeType);
        }

        var createFunctionAttributes = rangeType
            .GetAttributes()
            .Where(ad =>
                CustomSymbolEqualityComparer.Default.Equals(
                    ad.AttributeClass, 
                    _wellKnownTypesMiscellaneous.CreateFunctionAttribute))
            .ToImmutableArray();
        
        if (!createFunctionAttributes.Any())
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, $"The container has to have at least one attribute of type \"{nameof(CreateFunctionAttribute)}\"."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);

        var takenMemberNames = new HashSet<string>();
        takenMemberNames.UnionWith(rangeType.MemberNames);

        foreach (var createFunctionAttribute in createFunctionAttributes)
        {
            var location = createFunctionAttribute.GetLocation();
            if (createFunctionAttribute.ConstructorArguments.Length == 3
                && createFunctionAttribute.ConstructorArguments[1].Value is string functionName)
            {
                if (functionName == nameof(IDisposable.Dispose))
                    LocalDiagLogger.Error(
                        ValidationErrorDiagnostic(rangeType, rangeType, $"Create function isn't allowed to have the name \"{nameof(IDisposable.Dispose)}\", because a method with that name may have to be generated by the container."),
                        location);
                if (functionName == nameof(IAsyncDisposable.DisposeAsync))
                    LocalDiagLogger.Error(
                        ValidationErrorDiagnostic(rangeType, rangeType, $"Create function isn't allowed to have the name \"{nameof(IAsyncDisposable.DisposeAsync)}\", because a method with that name will be generated by the container."), 
                        location);

                foreach (var concreteFunctionName in new [] { $"{functionName}{Constants.CreateFunctionSuffix}", $"{functionName}{Constants.CreateFunctionSuffixAsync}", $"{functionName}{Constants.CreateFunctionSuffixValueAsync}"})
                {
                    if (takenMemberNames.Contains(concreteFunctionName))
                        LocalDiagLogger.Error(
                            ValidationErrorDiagnostic(rangeType, rangeType, $"Create function's name \"{concreteFunctionName}\" collides with one of the other members of the container class."), 
                            location);
                    takenMemberNames.Add(concreteFunctionName);
                }
            }
            else
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, rangeType, "Attribute doesn't have expected constructor arguments."), 
                    location);
        }

        void ValidateCustomScope(INamedTypeSymbol customScope, ISet<INamedTypeSymbol> customScopeTypesSet)
        {
            var customScopeAttributes = customScope
                .GetAttributes()
                .Where(ad => CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                    _wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
                .ToArray();

            if (customScopeAttributes.Length == 0)
            {
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, $"Custom scope \"{customScope.Name}\" has to have at least one \"{_wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute.FullName()}\"-attribute."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);
            }

            var scopeRootTypes = customScopeAttributes
                .SelectMany(ad => ad.ConstructorArguments[0].Values)
                .Select(tc => tc.Value)
                .OfType<INamedTypeSymbol>()
                .ToList();
            foreach (var scopeRootType in scopeRootTypes
                         .Where(scopeRootType => customScopeTypesSet.Contains(scopeRootType, CustomSymbolEqualityComparer.Default)))
            {
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, $"Custom scope \"{customScope.Name}\" gets the type \"{scopeRootType.FullName()}\" passed into one of its \"{_wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute.FullName()}\"-attributes, but it is already in use in another custom scope."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);
            }
            customScopeTypesSet.UnionWith(scopeRootTypes);
        }
    }

    protected override DiagLogData ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol _, string specification) => 
        ErrorLogData.ValidationContainer(rangeType, specification);
}