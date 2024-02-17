using System.Text.RegularExpressions;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateRange
{
    void Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType);
}

internal abstract class ValidateRange : IValidateRange
{
    private readonly IValidateUserDefinedAddForDisposalSync _validateUserDefinedAddForDisposalSync;
    private readonly IValidateUserDefinedAddForDisposalAsync _validateUserDefinedAddForDisposalAsync;
    private readonly IValidateUserDefinedConstructorParametersInjectionMethod _validateUserDefinedConstructorParametersInjectionMethod;
    private readonly IValidateUserDefinedPropertiesMethod _validateUserDefinedPropertiesMethod;
    private readonly IValidateUserDefinedInitializerParametersInjectionMethod _validateUserDefinedInitializerParametersInjectionMethod;
    private readonly IValidateUserDefinedFactoryMethod _validateUserDefinedFactoryMethod;
    private readonly IValidateUserDefinedFactoryField _validateUserDefinedFactoryField;
    private readonly IValidateAttributes _validateAttributes;
    protected readonly ILocalDiagLogger LocalDiagLogger;
    private readonly IRangeUtility _rangeUtility;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private readonly Regex _generatedMemberNames = new("(_[1-9][0-9]*){2}$");

    internal ValidateRange(
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        IValidateAttributes validateAttributes,
        IContainerWideContext containerWideContext,
        ILocalDiagLogger localDiagLogger,
        IRangeUtility rangeUtility)
    {
        _validateUserDefinedAddForDisposalSync = validateUserDefinedAddForDisposalSync;
        _validateUserDefinedAddForDisposalAsync = validateUserDefinedAddForDisposalAsync;
        _validateUserDefinedConstructorParametersInjectionMethod = validateUserDefinedConstructorParametersInjectionMethod;
        _validateUserDefinedPropertiesMethod = validateUserDefinedPropertiesMethod;
        _validateUserDefinedInitializerParametersInjectionMethod = validateUserDefinedInitializerParametersInjectionMethod;
        _validateUserDefinedFactoryMethod = validateUserDefinedFactoryMethod;
        _validateUserDefinedFactoryField = validateUserDefinedFactoryField;
        _validateAttributes = validateAttributes;
        LocalDiagLogger = localDiagLogger;
        _rangeUtility = rangeUtility;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
    }

    protected abstract DiagLogData ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol container, string specification);

    protected DiagLogData ValidationErrorDiagnosticBase(INamedTypeSymbol baseType, INamedTypeSymbol rangeType, string specification) => 
        ErrorLogData.ValidationBaseClass(baseType, rangeType, specification);
    
    private void LogError(INamedTypeSymbol rangeType, INamedTypeSymbol containerType, string specification) =>
        LocalDiagLogger.Error(
            ValidationErrorDiagnostic(rangeType, containerType, specification),
            rangeType.Locations.FirstOrDefault() ?? Location.None);
    
    private void LogErrorBase(INamedTypeSymbol baseType, INamedTypeSymbol rangeType, string specification) =>
        LocalDiagLogger.Error(
            ValidationErrorDiagnosticBase(baseType, rangeType, specification),
            baseType.Locations.FirstOrDefault() ?? Location.None);

    public virtual void Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        ValidateRangeAndBaseType(rangeType, false);
        
        if (!rangeType.IsSealed) LogError(rangeType, containerType, "Has to be a sealed type.");
        
        var unfilteredMembers = _rangeUtility.GetUnfilteredMembers(rangeType);

        foreach (var baseType in GetBaseTypes())
        {
            ValidateRangeAndBaseType(baseType, true);
            if (baseType.IsSealed) LogErrorBase(baseType, rangeType, "Has to be a non-sealed base type.");
        }

        if (rangeType.AllInterfaces.Any(nts => 
                CustomSymbolEqualityComparer.Default.Equals(nts, _wellKnownTypes.IDisposable)))
            LogError(rangeType, containerType, $"Isn't allowed to implement the interface {_wellKnownTypes.IDisposable.FullName()}. It'll be generated by DIE.");

        if (rangeType.AllInterfaces.Any(nts =>
                CustomSymbolEqualityComparer.Default.Equals(nts, _wellKnownTypes.IAsyncDisposable)))
            LogError(rangeType, containerType, $"Isn't allowed to implement the interface {_wellKnownTypes.IAsyncDisposable.FullName()}. It'll be generated by DIE.");

        ValidateAddForDisposal(Constants.UserDefinedAddForDisposal, true);
        ValidateAddForDisposal(Constants.UserDefinedAddForDisposalAsync, false);

        ValidateUserDefinedInjection(
            Constants.UserDefinedConstrParams, 
            _validateUserDefinedConstructorParametersInjectionMethod);
        ValidateUserDefinedInjection(
            Constants.UserDefinedProps, 
            _validateUserDefinedPropertiesMethod);
        ValidateUserDefinedInjection(
            Constants.UserDefinedInitParams,
            _validateUserDefinedInitializerParametersInjectionMethod);

        var userDefinedFactories = unfilteredMembers
            .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory, StringComparison.Ordinal))
            .ToImmutableArray();
        
        if (userDefinedFactories.Any())
        {
            foreach (var symbol in userDefinedFactories
                         .Where(s => s is not (IFieldSymbol or IMethodSymbol or IPropertySymbol)))
                LogError(rangeType, containerType, $"Member \"{symbol.Name}\" should be a field variable, a property, or a method but isn't.");

            foreach (var userDefinedFactoryMethod in userDefinedFactories
                         .OfType<IMethodSymbol>())
                _validateUserDefinedFactoryMethod.Validate(userDefinedFactoryMethod, rangeType, containerType);

            foreach (var userDefinedFactoryProperty in userDefinedFactories
                         .OfType<IPropertySymbol>())
            {
                if (userDefinedFactoryProperty.GetMethod is { } getter)
                    _validateUserDefinedFactoryMethod.Validate(getter, rangeType, containerType);
                else
                    LogError(rangeType, containerType, $"Factory property \"{userDefinedFactoryProperty.Name}\" has to have a getter method.");
            }

            foreach (var userDefinedFactoryField in userDefinedFactories.OfType<IFieldSymbol>())
                _validateUserDefinedFactoryField.Validate(userDefinedFactoryField, rangeType, containerType);
        }

        foreach (var initializedInstancesAttribute in GetInitializedInstancesAttributes())
        {
            if (initializedInstancesAttribute
                .ConstructorArguments[0]
                .Values
                .Select(v => v.Value)
                .Any(o => o is not INamedTypeSymbol type || !_validateAttributes.ValidateImplementation(type)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(
                        rangeType, 
                        containerType, 
                        "Initialized instance attribute is only allowed to have implementation types passed to."),
                    initializedInstancesAttribute.GetLocation());
        }

        return;

        void ValidateRangeAndBaseType(INamedTypeSymbol type, bool isBaseType)
        {
            Action<string> log = isBaseType 
                ? s => LogErrorBase(type, rangeType, s) 
                : s => LogError(rangeType, containerType, s);

            if (type.IsAbstract != isBaseType) log(isBaseType ? "Has to be abstract." : "Has to be non-abstract.");
            if (type.IsExtern) log("Has to be non-extern.");
            if (type.IsUnmanagedType) log("Has to be a managed type.");
            if (type.IsComImport) log("Has to be no COM import.");
            if (type.IsImplicitClass) log("Has to be non-implicit.");
            if (type.IsUnboundGenericType) log("Has to be no unbound generic type.");
            if (type.IsScriptClass) log("Has to be no script class.");
            if (type.IsNamespace) log("Has to be no namespace.");
            if (type.IsRecord) log("Has to be no record.");
            if (type.IsStatic) log("Has to be non-static.");
            if (type.IsVirtual) log("Has to be non-virtual.");
            if (type.IsAnonymousType) log("Has to be non-anonymous.");
            if (type.IsTupleType) log("Has to be no tuple type.");
            if (type.IsReadOnly) log("Has to be non-read-only.");;
            if (type.IsImplicitlyDeclared) log("Has to be non-implicitly-declared.");
            if (type.IsNativeIntegerType) log("Has to be no native integer type.");
            if (type.MightContainExtensionMethods) log("Isn't allowed to contain extension methods.");
            if (type.EnumUnderlyingType is not null) log("Isn't allowed to have an underlying enum type.");
            if (type.NativeIntegerUnderlyingType is not null) log("Isn't allowed to have an underlying native integer type.");
            if (type.TupleUnderlyingType is not null) log("Isn't allowed to have an underlying tuple type.");
            if (!type.IsReferenceType) log("Has to be a reference type.");
            if (!type.IsType) log("Has to be a type.");
            if (!type.CanBeReferencedByName) log("Has to be referable by name.");
            if (type.TypeKind != TypeKind.Class) log("Has to be a class.");
            
            if (type.GetMembers().Any(m => _generatedMemberNames.IsMatch(m.Name)))
                log("Isn't allowed to contain user-defined members which match regex \"(_[1-9][0-9]*){2}$\".");
            if (type.GetTypeMembers().Any(m => _generatedMemberNames.IsMatch(m.Name)))
                log("Isn't allowed to contain user-defined type members which match regex \"(_[1-9][0-9]*){2}$\".");

            if (rangeType.MemberNames.Contains(nameof(IDisposable.Dispose)))
                log($"Isn't allowed to contain an user-defined member \"{nameof(IDisposable.Dispose)}\", because a method with that name may have to be generated by the container.");
            if (rangeType.MemberNames.Contains(Constants.IAsyncDisposableDisposeAsync))
                log($"Isn't allowed to contain an user-defined member \"{Constants.IAsyncDisposableDisposeAsync}\", because a method with that name will be generated by the container.");
        }

        IEnumerable<INamedTypeSymbol> GetBaseTypes() => rangeType
            .AllBaseTypes()
            .Where(t => !CustomSymbolEqualityComparer.Default.Equals(t, _wellKnownTypes.Object));

        void ValidateAddForDisposal(string addForDisposalName, bool isSync)
        {
            var addForDisposalMembers = rangeType
                .GetMembers()
                .Where(s => s.Name == addForDisposalName)
                .ToImmutableArray();

            if (addForDisposalMembers.Length > 1)
                LogError(rangeType, containerType, $"Only a single \"{addForDisposalName}\"-member is allowed.");
            else if (addForDisposalMembers.Length == 1 && addForDisposalMembers[0] is not IMethodSymbol)
                LogError(rangeType, containerType, $"The \"{addForDisposalName}\"-member is required to be a method.");
            else if (addForDisposalMembers.Length == 1)
                if (addForDisposalMembers[0] is IMethodSymbol addForDisposalMember)
                {
                    if (isSync)
                        _validateUserDefinedAddForDisposalSync.Validate(
                            addForDisposalMember,
                            rangeType,
                            containerType);
                    else
                        _validateUserDefinedAddForDisposalAsync.Validate(
                            addForDisposalMember,
                            rangeType,
                            containerType);
                }
        }

        void ValidateUserDefinedInjection(string prefix, IValidateUserDefinedInjectionMethod validateUserDefinedInjectionMethod)
        {
            var userDefinedInjectionMethods = unfilteredMembers
                .Where(s => s.Name.StartsWith(prefix, StringComparison.Ordinal))
                .ToImmutableArray();

            if (userDefinedInjectionMethods.Any())
            {
                foreach (var symbol in userDefinedInjectionMethods.Where(s => s is not IMethodSymbol))
                    LogError(rangeType, containerType, $"Member \"{symbol.Name}\" should be a method but isn't.");

                foreach (var userDefinedPropertyMethod in userDefinedInjectionMethods.OfType<IMethodSymbol>())
                    validateUserDefinedInjectionMethod.Validate(
                        userDefinedPropertyMethod, 
                        rangeType,
                        containerType);
            }
        }

        IEnumerable<AttributeData> GetInitializedInstancesAttributes() => _rangeUtility
            .GetRangeAttributes(rangeType)
            .Where(ad => CustomSymbolEqualityComparer.Default.Equals(
                             ad.AttributeClass,
                             _wellKnownTypesMiscellaneous.InitializedInstancesAttribute)
                         && ad.ConstructorArguments.Length == 1
                         && ad.ConstructorArguments[0].Kind == TypedConstantKind.Array);
    }
}