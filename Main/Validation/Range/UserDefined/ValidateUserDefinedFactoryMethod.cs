namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedFactoryMethod : IValidateUserDefinedMethod
{
    
}

internal class ValidateUserDefinedFactoryMethod : ValidateUserDefinedMethod, IValidateUserDefinedFactoryMethod
{
    public override IEnumerable<Diagnostic> Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        foreach (var diagnostic in base.Validate(method, rangeType, containerType))
            yield return diagnostic;

        if (method is
            {
                IsPartialDefinition: false,
                ReturnsVoid: false,
                MethodKind: MethodKind.Ordinary or MethodKind.PropertyGet
            }
            && method.Parameters.All(p => p is
            {
                IsDiscard: false,
                IsOptional: false,
                IsParams: false,
                IsThis: false,
                RefKind: RefKind.None,
                HasExplicitDefaultValue: false
            }))
        {
        }
        
        if (method.IsPartialDefinition)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be partial.");
        
        if (method.ReturnsVoid)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to return void.");
        
        if (method.Parameters.Any(p => p.IsDiscard))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a discard parameter.");
        
        if (method.Parameters.Any(p => p.IsOptional))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a optional parameter.");
        
        if (method.Parameters.Any(p => p.IsParams))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a params parameter.");
        
        if (method.Parameters.Any(p => p.IsThis))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a this parameter.");
        
        if (method.Parameters.Any(p => p.RefKind != RefKind.None))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "All parameters should be either ordinary or an out parameter.");
        
        if (method.Parameters.Any(p => p.HasExplicitDefaultValue))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a parameter which has an explicit default value.");
        
        if (method.MethodKind != MethodKind.Ordinary && method.MethodKind != MethodKind.PropertyGet)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be either an ordinary method or a property getter.");
    }
}