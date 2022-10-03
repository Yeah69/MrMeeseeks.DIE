using System.Reflection.Metadata;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedMethod
{
    IEnumerable<Diagnostic> Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType);
}

internal abstract class ValidateUserDefinedMethod : IValidateUserDefinedMethod
{
    public virtual IEnumerable<Diagnostic> Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        if (method is
            {
                DeclaredAccessibility: Accessibility.Private,
                Arity: 0,
                CallingConvention: SignatureCallingConvention.Default,
                IsAsync: false,
                IsConditional: false,
                IsVararg: false,
                IsExtensionMethod: false,
                IsGenericMethod: false,
                IsInitOnly: false,
                IsStatic: false,
                IsImplicitlyDeclared: false
            })
        {
        }
        
        if (method.DeclaredAccessibility != Accessibility.Private)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be private.");
        
        if (method.Arity != 0)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to have an arity of zero.");
        
        if (method.CallingConvention != SignatureCallingConvention.Default)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to have a default calling signature.");
        
        if (method.IsAsync)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be async.");
        
        if (method.IsConditional)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be marked with the ConditionalAttribute.");
        
        if (method.IsVararg)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be CLI VARAG.");
        
        if (method.IsExtensionMethod)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be an extension method.");
        
        if (method.IsInitOnly)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be an init method.");
        
        if (method.IsStatic)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be static.");
        
        if (method.IsImplicitlyDeclared)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be implicitly declared.");
    }

    protected static Diagnostic ValidationErrorDiagnostic(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol container, string specification) =>
        Diagnostics.ValidationUserDefinedElement(method, rangeType, container, specification, ExecutionPhase.Validation);
}