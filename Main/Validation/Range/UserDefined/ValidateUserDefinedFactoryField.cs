using MrMeeseeks.DIE.Logging;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedFactoryField
{
    void Validate(IFieldSymbol field, INamedTypeSymbol rangeType, INamedTypeSymbol containerType);
}

internal class ValidateUserDefinedFactoryField : IValidateUserDefinedFactoryField
{
    private readonly ILocalDiagLogger _localDiagLogger;

    internal ValidateUserDefinedFactoryField(
        ILocalDiagLogger localDiagLogger) =>
        _localDiagLogger = localDiagLogger;

    public void Validate(IFieldSymbol field, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
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
            _localDiagLogger.Error(
                ValidationErrorDiagnostic(field, rangeType, containerType, "Has to be private."),
                field.Locations.FirstOrDefault() ?? Location.None);
        
        if (field.IsStatic)
            _localDiagLogger.Error(
                ValidationErrorDiagnostic(field, rangeType, containerType, "Isn't allowed to be static."),
                field.Locations.FirstOrDefault() ?? Location.None);
        
        if (field.IsImplicitlyDeclared)
            _localDiagLogger.Error(
                ValidationErrorDiagnostic(field, rangeType, containerType, "Isn't allowed to be implicitly declared."),
                field.Locations.FirstOrDefault() ?? Location.None);
    }

    private static DiagLogData ValidationErrorDiagnostic(IFieldSymbol field, INamedTypeSymbol rangeType, INamedTypeSymbol container, string specification) =>
        ErrorLogData.ValidationUserDefinedElement(field, rangeType, container, specification);
}