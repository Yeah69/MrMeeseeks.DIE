using MrMeeseeks.DIE.Logging;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedFactoryMethod : IValidateUserDefinedMethod;

internal sealed class ValidateUserDefinedFactoryMethod : ValidateUserDefinedMethod, IValidateUserDefinedFactoryMethod
{
    internal ValidateUserDefinedFactoryMethod(
        ILocalDiagLogger localDiagLogger) 
        : base(localDiagLogger)
    {
    }
    
    public override void Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        base.Validate(method, rangeType, containerType);

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
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to be partial."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.ReturnsVoid)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to return void."),
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
        
        if (method.Parameters.Any(p => p.RefKind != RefKind.None))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "All parameters should be either ordinary or an out parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.Parameters.Any(p => p.HasExplicitDefaultValue))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a parameter which has an explicit default value."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.MethodKind != MethodKind.Ordinary && method.MethodKind != MethodKind.PropertyGet)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be either an ordinary method or a property getter."),
                method.Locations.FirstOrDefault() ?? Location.None);
    }
}