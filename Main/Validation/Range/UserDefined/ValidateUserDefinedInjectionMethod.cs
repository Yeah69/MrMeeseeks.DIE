using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedInjectionMethod : IValidateUserDefinedMethod
{
    
}

internal abstract class ValidateUserDefinedInjectionMethod : ValidateUserDefinedMethod, IValidateUserDefinedInjectionMethod
{
    protected abstract INamedTypeSymbol InjectionAttribute { get; }

    internal ValidateUserDefinedInjectionMethod(
        ILocalDiagLogger localDiagLogger) 
        : base(localDiagLogger)
    {
    }

    public override void Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        base.Validate(method, rangeType, containerType);

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
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Has to have at least one out parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.IsPartialDefinition)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be partial."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (!method.ReturnsVoid)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a return type."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.IsDiscard))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a discard parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.IsOptional))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a optional parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.IsParams))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a params parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.IsThis))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a this parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.RefKind != RefKind.None && p.RefKind != RefKind.Out))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "All parameters should be either ordinary or an out parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.HasExplicitDefaultValue))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a parameter which has an explicit default value."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.MethodKind != MethodKind.Ordinary)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be an ordinary method."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (!method.CanBeReferencedByName)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Should be able to be referenced by name."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method
                .GetAttributes()
                .Count(ad => CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass, InjectionAttribute))
                != 1)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, $"Has to have exactly one attribute of type \"{InjectionAttribute.FullName()}\"."),
                method.Locations.FirstOrDefault() ?? Location.None);
    }
}