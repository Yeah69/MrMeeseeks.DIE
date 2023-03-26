using System.Text.RegularExpressions;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
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
        ILocalDiagLogger localDiagLogger)
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
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
    }

    protected abstract DiagLogData ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol container, string specification);

    public virtual void Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        if (rangeType is
            {
                // What it shouldn't be
                IsAbstract: false,
                IsExtern: false,
                IsComImport: false,
                IsImplicitClass: false,
                IsUnboundGenericType: false,
                IsScriptClass: false,
                IsNamespace: false,
                IsRecord: false,
                IsStatic: false,
                IsVirtual: false,
                IsAnonymousType: false,
                IsTupleType: false,
                IsUnmanagedType: false,
                IsValueType: false,
                IsReadOnly: false,
                IsImplicitlyDeclared: false,
                IsNativeIntegerType: false,
                MightContainExtensionMethods: false,
                EnumUnderlyingType: null,
                NativeIntegerUnderlyingType: null,
                TupleUnderlyingType: null,

                // What it should be
                IsType: true,
                IsReferenceType: true,
                TypeKind: TypeKind.Class,
                CanBeReferencedByName: true,
            })
        {
            if (rangeType
                .GetMembers()
                .Any(m => _generatedMemberNames.IsMatch(m.Name)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to contain user-defined members which match regex \"(_[1-9][0-9]*){2}$\"."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);
            
            if (rangeType
                .GetTypeMembers()
                .Any(m => _generatedMemberNames.IsMatch(m.Name)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to contain user-defined type members which match regex \"(_[1-9][0-9]*){2}$\"."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);

            if (rangeType.AllInterfaces.Any(nts =>
                    CustomSymbolEqualityComparer.Default.Equals(nts, _wellKnownTypes.IDisposable)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to implement the interface {_wellKnownTypes.IDisposable.FullName()}. It'll be generated by DIE."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);

            if (rangeType
                .MemberNames
                .Contains(nameof(IDisposable.Dispose)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to contain an user-defined member \"{nameof(IDisposable.Dispose)}\", because a method with that name may have to be generated by the container."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);

            if (rangeType.AllInterfaces.Any(nts =>
                    CustomSymbolEqualityComparer.Default.Equals(nts, _wellKnownTypes.IAsyncDisposable)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to implement the interface {_wellKnownTypes.IAsyncDisposable.FullName()}. It'll be generated by DIE."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);

            if (rangeType
                .MemberNames
                .Contains(nameof(IAsyncDisposable.DisposeAsync)))
                LocalDiagLogger.Error(
                    ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to contain an user-defined member \"{nameof(IAsyncDisposable.DisposeAsync)}\", because a method with that name will be generated by the container."),
                    rangeType.Locations.FirstOrDefault() ?? Location.None);

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

            var userDefinedFactories = rangeType
                .GetMembers()
                .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                .ToImmutableArray();
            
            if (userDefinedFactories.Any())
            {
                foreach (var symbol in userDefinedFactories
                             .Where(s => s is not (IFieldSymbol or IMethodSymbol or IPropertySymbol)))
                    LocalDiagLogger.Error(
                        ValidationErrorDiagnostic(rangeType, containerType, $"Member \"{symbol.Name}\" should be a field variable, a property, or a method but isn't."),
                        rangeType.Locations.FirstOrDefault() ?? Location.None);

                foreach (var userDefinedFactoryMethod in userDefinedFactories
                             .OfType<IMethodSymbol>())
                    _validateUserDefinedFactoryMethod.Validate(userDefinedFactoryMethod, rangeType, containerType);

                foreach (var userDefinedFactoryProperty in userDefinedFactories
                             .OfType<IPropertySymbol>())
                {
                    if (userDefinedFactoryProperty.GetMethod is { } getter)
                        _validateUserDefinedFactoryMethod.Validate(getter, rangeType, containerType);
                    else
                        LocalDiagLogger.Error(
                            ValidationErrorDiagnostic(rangeType, containerType, $"Factory property \"{userDefinedFactoryProperty.Name}\" has to have a getter method."),
                            rangeType.Locations.FirstOrDefault() ?? Location.None);
                }

                foreach (var userDefinedFactoryField in userDefinedFactories
                             .OfType<IFieldSymbol>())
                    _validateUserDefinedFactoryField.Validate(userDefinedFactoryField, rangeType, containerType);
            }

            foreach (var initializedInstancesAttribute in rangeType
                         .GetAttributes()
                         .Where(ad => CustomSymbolEqualityComparer.Default.Equals(
                                          ad.AttributeClass, 
                                          _wellKnownTypesMiscellaneous.InitializedInstancesAttribute) 
                                      && ad.ConstructorArguments.Length == 1 
                                      && ad.ConstructorArguments[0].Kind == TypedConstantKind.Array))
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

            void ValidateAddForDisposal(string addForDisposalName, bool isSync)
            {
                var addForDisposalMembers = rangeType
                    .GetMembers()
                    .Where(s => s.Name == addForDisposalName)
                    .ToImmutableArray();

                if (addForDisposalMembers.Length > 1)
                    LocalDiagLogger.Error(
                        ValidationErrorDiagnostic(rangeType, containerType, $"Only a single \"{addForDisposalName}\"-member is allowed."),
                        rangeType.Locations.FirstOrDefault() ?? Location.None);
                else if (addForDisposalMembers.Length == 1)
                {
                    if (addForDisposalMembers[0] is not IMethodSymbol)
                        LocalDiagLogger.Error(
                            ValidationErrorDiagnostic(rangeType, containerType, $"The \"{addForDisposalName}\"-member is required to be a method."),
                            rangeType.Locations.FirstOrDefault() ?? Location.None);
                    else if (addForDisposalMembers[0] is IMethodSymbol addForDisposalMember)
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
            }

            void ValidateUserDefinedInjection(string prefix, IValidateUserDefinedInjectionMethod validateUserDefinedInjectionMethod)
            {
                var userDefinedInjectionMethods = rangeType
                    .GetMembers()
                    .Where(s => s.Name.StartsWith(prefix))
                    .ToImmutableArray();

                if (userDefinedInjectionMethods.Any())
                {
                    foreach (var symbol in userDefinedInjectionMethods
                                 .Where(s => s is not IMethodSymbol))
                        LocalDiagLogger.Error(
                            ValidationErrorDiagnostic(rangeType, containerType, $"Member \"{symbol.Name}\" should be a method but isn't."),
                            rangeType.Locations.FirstOrDefault() ?? Location.None);

                    foreach (var userDefinedPropertyMethod in userDefinedInjectionMethods.OfType<IMethodSymbol>())
                        validateUserDefinedInjectionMethod.Validate(
                            userDefinedPropertyMethod, 
                            rangeType,
                            containerType);
                }
            }
        }
        
        if (rangeType.IsAbstract)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-abstract."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsExtern)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-extern."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsComImport)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no COM import."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsImplicitClass)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-implicit."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsUnboundGenericType)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no unbound generic type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsScriptClass)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no script class."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsNamespace)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no namespace."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsRecord)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no record."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsStatic)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-static."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsVirtual)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-virtual."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsAnonymousType)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-anonymous."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsTupleType)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no tuple type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsReadOnly)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-read-only."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsImplicitlyDeclared)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-implicitly-declared."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.IsNativeIntegerType)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be no native integer type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.MightContainExtensionMethods)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to contain extension types."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.EnumUnderlyingType is {})
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to have an underlying enum type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.NativeIntegerUnderlyingType is {})
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to have an underlying native integer type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.TupleUnderlyingType is {})
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to have an underlying tuple type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (!rangeType.IsType)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be a type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (!rangeType.IsReferenceType)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be a reference type."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (!rangeType.CanBeReferencedByName)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be referable by name."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
        
        if (rangeType.TypeKind != TypeKind.Class)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(rangeType, containerType, "Has to be a class."),
                rangeType.Locations.FirstOrDefault() ?? Location.None);
    }
}