using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedInjectionMethod : IValidateUserDefinedMethod
{
    
}

internal abstract class ValidateUserDefinedInjectionMethod : ValidateUserDefinedMethod, IValidateUserDefinedInjectionMethod
{
    protected abstract INamedTypeSymbol InjectionAttribute { get; }

    public override IEnumerable<Diagnostic> Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        foreach (var diagnostic in base.Validate(method, rangeType, containerType))
            yield return diagnostic;

        if (method is
            {
                Parameters.Length: > 0,
                IsPartialDefinition: false,
                ReturnsVoid: true,
                MethodKind: MethodKind.Ordinary,
                CanBeReferencedByName: true
            }
            && method.Parameters.All(p => p is
            {
                IsDiscard: false,
                IsOptional: false,
                IsParams: false,
                IsThis: false,
                RefKind: RefKind.None or RefKind.Out,
                HasExplicitDefaultValue: false
            })
            && method.Parameters.Any(p => p.RefKind == RefKind.Out))
        {
        }
        
        if (!method.Parameters.Any(p => p.RefKind == RefKind.Out))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to have at least one out parameter.");
        
        if (method.IsPartialDefinition)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be partial.");
        
        if (!method.ReturnsVoid)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a return type.");
        
        if (method.Parameters.Any(p => p.IsDiscard))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a discard parameter.");
        
        if (method.Parameters.Any(p => p.IsOptional))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a optional parameter.");
        
        if (method.Parameters.Any(p => p.IsParams))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a params parameter.");
        
        if (method.Parameters.Any(p => p.IsThis))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a this parameter.");
        
        if (method.Parameters.Any(p => p.RefKind != RefKind.None && p.RefKind != RefKind.Out))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "All parameters should be either ordinary or an out parameter.");
        
        if (method.Parameters.Any(p => p.HasExplicitDefaultValue))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a parameter which has an explicit default value.");
        
        if (method.MethodKind != MethodKind.Ordinary)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be an ordinary method.");
        
        if (!method.CanBeReferencedByName)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Should be able to be referenced by name.");
        
        if (method
                .GetAttributes()
                .Count(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, InjectionAttribute))
                != 1)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, $"Has to have exactly one attribute of type \"{InjectionAttribute.FullName()}\".");
    }
}