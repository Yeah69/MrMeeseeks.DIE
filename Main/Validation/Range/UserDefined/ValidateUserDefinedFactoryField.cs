namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedFactoryField
{
    IEnumerable<Diagnostic> Validate(IFieldSymbol field, INamedTypeSymbol rangeType, INamedTypeSymbol containerType);
}

internal class ValidateUserDefinedFactoryField : IValidateUserDefinedFactoryField
{
    public IEnumerable<Diagnostic> Validate(IFieldSymbol field, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        if (field is
            {
                DeclaredAccessibility: Accessibility.Private,
                IsStatic: false,
                IsImplicitlyDeclared: false
            })
        {
        }
        
        if (field.DeclaredAccessibility != Accessibility.Private)
            yield return ValidationErrorDiagnostic(field, rangeType, containerType, "Has to be private.");
        
        if (field.IsStatic)
            yield return ValidationErrorDiagnostic(field, rangeType, containerType, "Isn't allowed to be static.");
        
        if (field.IsImplicitlyDeclared)
            yield return ValidationErrorDiagnostic(field, rangeType, containerType, "Isn't allowed to be implicitly declared.");
    }

    protected static Diagnostic ValidationErrorDiagnostic(IFieldSymbol field, INamedTypeSymbol rangeType, INamedTypeSymbol container, string specification) =>
        Diagnostics.ValidationUserDefinedElement(field, rangeType, container, specification, ExecutionPhase.Validation);
}