using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateScopeBase : IValidateRange
{
}

internal abstract class ValidateScopeBase : ValidateRange, IValidateScopeBase
{
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private readonly IImmutableSet<INamedTypeSymbol> _notAllowedAttributeTypes;

    internal ValidateScopeBase(
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        IValidateAttributes validateAttributes,
        IContainerWideContext containerWideContext) 
        : base(
            validateUserDefinedAddForDisposalSync, 
            validateUserDefinedAddForDisposalAsync,
            validateUserDefinedConstructorParametersInjectionMethod,
            validateUserDefinedPropertiesMethod,
            validateUserDefinedInitializerParametersInjectionMethod,
            validateUserDefinedFactoryMethod,
            validateUserDefinedFactoryField,
            validateAttributes,
            containerWideContext)
    {
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;

        var wellKnownTypesAggregation = containerWideContext.WellKnownTypesAggregation;
        _notAllowedAttributeTypes = ImmutableHashSet.Create<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            wellKnownTypesAggregation.ContainerInstanceAbstractionAggregationAttribute,
            wellKnownTypesAggregation.ContainerInstanceImplementationAggregationAttribute,
            wellKnownTypesAggregation.FilterContainerInstanceAbstractionAggregationAttribute,
            wellKnownTypesAggregation.FilterContainerInstanceImplementationAggregationAttribute,
            wellKnownTypesAggregation.TransientScopeInstanceAbstractionAggregationAttribute,
            wellKnownTypesAggregation.TransientScopeInstanceImplementationAggregationAttribute,
            wellKnownTypesAggregation.FilterTransientScopeInstanceAbstractionAggregationAttribute,
            wellKnownTypesAggregation.FilterTransientScopeInstanceImplementationAggregationAttribute,
            wellKnownTypesAggregation.TransientScopeRootAbstractionAggregationAttribute,
            wellKnownTypesAggregation.TransientScopeRootImplementationAggregationAttribute,
            wellKnownTypesAggregation.FilterTransientScopeRootAbstractionAggregationAttribute,
            wellKnownTypesAggregation.FilterTransientScopeRootImplementationAggregationAttribute,
            wellKnownTypesAggregation.ScopeRootAbstractionAggregationAttribute,
            wellKnownTypesAggregation.ScopeRootImplementationAggregationAttribute,
            wellKnownTypesAggregation.FilterScopeRootAbstractionAggregationAttribute,
            wellKnownTypesAggregation.FilterScopeRootImplementationAggregationAttribute);
    }

    protected abstract string DefaultScopeName { get; }
    
    protected abstract string CustomScopeName { get; }
    
    protected abstract string ScopeName { get; }

    public override IEnumerable<Diagnostic> Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        foreach (var diagnostic in base.Validate(rangeType, containerType))
            yield return diagnostic;

        if (rangeType.DeclaredAccessibility != Accessibility.Private)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be declared private.");
        
        if (rangeType.Name != DefaultScopeName && !rangeType.Name.StartsWith(CustomScopeName))
            yield return ValidationErrorDiagnostic(rangeType, containerType, $"{ScopeName}'s name hast to be either \"{DefaultScopeName}\" if it is the default {ScopeName} or start with \"{CustomScopeName}\" if it is a custom {ScopeName}.");
        
        foreach (var notAllowedAttributeType in rangeType
                     .GetAttributes()
                     .Select(ad => ad.AttributeClass)
                     .Distinct(CustomSymbolEqualityComparer.Default)
                     .OfType<INamedTypeSymbol>()
                     .Where(nts => _notAllowedAttributeTypes.Contains(nts, CustomSymbolEqualityComparer.Default)))
            yield return ValidationErrorDiagnostic(rangeType, containerType, $"{ScopeName}s aren't allowed to have attributes of type \"{notAllowedAttributeType.FullName()}\".");

        var isDefault = rangeType.Name == DefaultScopeName;
        var isCustom = rangeType.Name.StartsWith(CustomScopeName);

        if (isDefault)
        {
            if (rangeType
                .GetAttributes()
                .Any(ad => CustomSymbolEqualityComparer.Default.Equals(
                    ad.AttributeClass,
                    _wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute)))
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"A default (Transient)Scope isn't allowed to have the attribute of type \"{_wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute.FullName()}\".");
        }

        if (isCustom)
        {
            if (rangeType
                .GetAttributes()
                .Count(ad => CustomSymbolEqualityComparer.Default.Equals(
                    ad.AttributeClass,
                    _wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute)) != 1)
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"A custom (Transient)Scope has to have exactly one attribute of type \"{_wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute.FullName()}\".");
        }
    }
}