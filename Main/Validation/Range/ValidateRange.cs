using System.Text.RegularExpressions;
using MrMeeseeks.DIE.Validation.Range.UserDefined;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range;

internal interface IValidateRange
{
    IEnumerable<Diagnostic> Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType);
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
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Regex _generatedMemberNames = new("(_[1-9][0-9]*){2}$");

    internal ValidateRange(
        IValidateUserDefinedAddForDisposalSync validateUserDefinedAddForDisposalSync,
        IValidateUserDefinedAddForDisposalAsync validateUserDefinedAddForDisposalAsync,
        IValidateUserDefinedConstructorParametersInjectionMethod validateUserDefinedConstructorParametersInjectionMethod,
        IValidateUserDefinedPropertiesMethod validateUserDefinedPropertiesMethod,
        IValidateUserDefinedInitializerParametersInjectionMethod validateUserDefinedInitializerParametersInjectionMethod,
        IValidateUserDefinedFactoryMethod validateUserDefinedFactoryMethod,
        IValidateUserDefinedFactoryField validateUserDefinedFactoryField,
        WellKnownTypes wellKnownTypes)
    {
        _validateUserDefinedAddForDisposalSync = validateUserDefinedAddForDisposalSync;
        _validateUserDefinedAddForDisposalAsync = validateUserDefinedAddForDisposalAsync;
        _validateUserDefinedConstructorParametersInjectionMethod = validateUserDefinedConstructorParametersInjectionMethod;
        _validateUserDefinedPropertiesMethod = validateUserDefinedPropertiesMethod;
        _validateUserDefinedInitializerParametersInjectionMethod = validateUserDefinedInitializerParametersInjectionMethod;
        _validateUserDefinedFactoryMethod = validateUserDefinedFactoryMethod;
        _validateUserDefinedFactoryField = validateUserDefinedFactoryField;
        _wellKnownTypes = wellKnownTypes;
    }

    protected abstract Diagnostic ValidationErrorDiagnostic(INamedTypeSymbol rangeType, INamedTypeSymbol container, string specification);

    public virtual IEnumerable<Diagnostic> Validate(INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
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
                yield return ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to contain user-defined members which match regex \"(_[1-9][0-9]*){2}$\".");
            
            if (rangeType
                .GetTypeMembers()
                .Any(m => _generatedMemberNames.IsMatch(m.Name)))
                yield return ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to contain user-defined type members which match regex \"(_[1-9][0-9]*){2}$\".");

            if (rangeType.AllInterfaces.Any(nts =>
                    CustomSymbolEqualityComparer.Default.Equals(nts, _wellKnownTypes.IDisposable)))
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to implement the interface {_wellKnownTypes.IDisposable.FullName()}. It'll be generated by DIE.");

            if (rangeType
                .MemberNames
                .Contains(nameof(IDisposable.Dispose)))
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to contain an user-defined member \"{nameof(IDisposable.Dispose)}\", because a method with that name may have to be generated by the container.");

            if (rangeType.AllInterfaces.Any(nts =>
                    CustomSymbolEqualityComparer.Default.Equals(nts, _wellKnownTypes.IAsyncDisposable)))
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to implement the interface {_wellKnownTypes.IAsyncDisposable.FullName()}. It'll be generated by DIE.");

            if (rangeType
                .MemberNames
                .Contains(nameof(IAsyncDisposable.DisposeAsync)))
                yield return ValidationErrorDiagnostic(rangeType, containerType, $"Isn't allowed to contain an user-defined member \"{nameof(IAsyncDisposable.DisposeAsync)}\", because a method with that name will be generated by the container.");
            
            foreach (var diagnostic in ValidateAddForDisposal(Constants.UserDefinedAddForDisposal, true))
                yield return diagnostic;
            foreach (var diagnostic in ValidateAddForDisposal(Constants.UserDefinedAddForDisposalAsync, false))
                yield return diagnostic;
            
            foreach (var diagnostic in ValidateUserDefinedInjection(Constants.UserDefinedConstrParams, _validateUserDefinedConstructorParametersInjectionMethod))
                yield return diagnostic;
            foreach (var diagnostic in ValidateUserDefinedInjection(Constants.UserDefinedProps, _validateUserDefinedPropertiesMethod))
                yield return diagnostic;
            foreach (var diagnostic in ValidateUserDefinedInjection(Constants.UserDefinedInitParams, _validateUserDefinedInitializerParametersInjectionMethod))
                yield return diagnostic;

            var userDefinedFactories = rangeType
                .GetMembers()
                .Where(s => s.Name.StartsWith(Constants.UserDefinedFactory))
                .ToImmutableArray();
            
            if (userDefinedFactories.Any())
            {
                foreach (var symbol in userDefinedFactories
                             .Where(s => s is not (IFieldSymbol or IMethodSymbol or IPropertySymbol)))
                    yield return ValidationErrorDiagnostic(rangeType, containerType, $"Member \"{symbol.Name}\" should be a field variable, a property, or a method but isn't.");

                foreach (var userDefinedFactoryMethod in userDefinedFactories
                             .OfType<IMethodSymbol>())
                    foreach (var diagnostic in _validateUserDefinedFactoryMethod.Validate(userDefinedFactoryMethod, rangeType, containerType))
                        yield return diagnostic;

                foreach (var userDefinedFactoryProperty in userDefinedFactories
                             .OfType<IPropertySymbol>())
                {
                    if (userDefinedFactoryProperty.GetMethod is { } getter)
                        foreach (var diagnostic in _validateUserDefinedFactoryMethod.Validate(getter, rangeType, containerType))
                            yield return diagnostic;
                    else
                        yield return ValidationErrorDiagnostic(rangeType, containerType, $"Factory property \"{userDefinedFactoryProperty.Name}\" has to have a getter method.");
                }
                
                foreach (var userDefinedFactoryField in userDefinedFactories
                             .OfType<IFieldSymbol>())
                    foreach (var diagnostic in _validateUserDefinedFactoryField.Validate(userDefinedFactoryField, rangeType, containerType))
                        yield return diagnostic;
            }

            IEnumerable<Diagnostic> ValidateAddForDisposal(string addForDisposalName, bool isSync)
            {
                var addForDisposalMembers = rangeType
                    .GetMembers()
                    .Where(s => s.Name == addForDisposalName)
                    .ToImmutableArray();

                if (addForDisposalMembers.Length > 1)
                    yield return ValidationErrorDiagnostic(rangeType, containerType, $"Only a single \"{addForDisposalName}\"-member is allowed.");
                else if (addForDisposalMembers.Length == 1)
                {
                    if (addForDisposalMembers[0] is not IMethodSymbol)
                        yield return ValidationErrorDiagnostic(rangeType, containerType, $"The \"{addForDisposalName}\"-member is required to be a method.");
                    else if (addForDisposalMembers[0] is IMethodSymbol addForDisposalMember)
                    {
                        if (isSync)
                            foreach (var diagnostic in _validateUserDefinedAddForDisposalSync.Validate(addForDisposalMember, rangeType, containerType))
                                yield return diagnostic;
                        else
                            foreach (var diagnostic in _validateUserDefinedAddForDisposalAsync.Validate(addForDisposalMember, rangeType, containerType))
                                yield return diagnostic;
                    }
                }
            }

            IEnumerable<Diagnostic> ValidateUserDefinedInjection(string prefix, IValidateUserDefinedInjectionMethod validateUserDefinedInjectionMethod)
            {
                var userDefinedInjectionMethods = rangeType
                    .GetMembers()
                    .Where(s => s.Name.StartsWith(prefix))
                    .ToImmutableArray();

                if (userDefinedInjectionMethods.Any())
                {
                    foreach (var symbol in userDefinedInjectionMethods
                                 .Where(s => s is not IMethodSymbol))
                        yield return ValidationErrorDiagnostic(rangeType, containerType, $"Member \"{symbol.Name}\" should be a method but isn't.");

                    foreach (var userDefinedPropertyMethod in userDefinedInjectionMethods.OfType<IMethodSymbol>())
                        foreach (var diagnostic in validateUserDefinedInjectionMethod.Validate(userDefinedPropertyMethod, rangeType, containerType))
                            yield return diagnostic;
                }
            }
        }
        
        if (rangeType.IsAbstract)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-abstract.");
        
        if (rangeType.IsExtern)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-extern.");
        
        if (rangeType.IsComImport)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no COM import.");
        
        if (rangeType.IsImplicitClass)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-implicit.");
        
        if (rangeType.IsUnboundGenericType)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no unbound generic type.");
        
        if (rangeType.IsScriptClass)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no script class.");
        
        if (rangeType.IsNamespace)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no namespace.");
        
        if (rangeType.IsRecord)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no record.");
        
        if (rangeType.IsStatic)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-static.");
        
        if (rangeType.IsVirtual)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-virtual.");
        
        if (rangeType.IsAnonymousType)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-anonymous.");
        
        if (rangeType.IsTupleType)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no tuple type.");
        
        if (rangeType.IsReadOnly)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-read-only.");
        
        if (rangeType.IsImplicitlyDeclared)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be non-implicitly-declared.");
        
        if (rangeType.IsNativeIntegerType)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be no native integer type.");
        
        if (rangeType.MightContainExtensionMethods)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to contain extension types.");
        
        if (rangeType.EnumUnderlyingType is {})
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to have an underlying enum type.");
        
        if (rangeType.NativeIntegerUnderlyingType is {})
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to have an underlying native integer type.");
        
        if (rangeType.TupleUnderlyingType is {})
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Isn't allowed to have an underlying tuple type.");
        
        if (!rangeType.IsType)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be a type.");
        
        if (!rangeType.IsReferenceType)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be a reference type.");
        
        if (!rangeType.CanBeReferencedByName)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be referable by name.");
        
        if (rangeType.TypeKind != TypeKind.Class)
            yield return ValidationErrorDiagnostic(rangeType, containerType, "Has to be a class.");
    }
}