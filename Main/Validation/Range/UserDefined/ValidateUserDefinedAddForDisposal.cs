using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedAddForDisposalSync : IValidateUserDefinedAddForDisposalBase;

internal sealed class ValidateUserDefinedAddForDisposalSync 
    : ValidateUserDefinedAddForDisposalBase, 
        IValidateUserDefinedAddForDisposalSync
{
    internal ValidateUserDefinedAddForDisposalSync(
        WellKnownTypes wellKnownTypes,
        ILocalDiagLogger localDiagLogger)
        : base(localDiagLogger) => 
        DisposableType = wellKnownTypes.IDisposable;

    protected override INamedTypeSymbol? DisposableType { get; }
}

internal interface IValidateUserDefinedAddForDisposalAsync : IValidateUserDefinedAddForDisposalBase;

internal sealed class ValidateUserDefinedAddForDisposalAsync : ValidateUserDefinedAddForDisposalBase,
    IValidateUserDefinedAddForDisposalAsync
{
    internal ValidateUserDefinedAddForDisposalAsync(
        WellKnownTypes wellKnownTypes,
        ILocalDiagLogger localDiagLogger)
        : base(localDiagLogger) => 
        DisposableType = wellKnownTypes.IAsyncDisposable;

    protected override INamedTypeSymbol? DisposableType { get; }
}

internal interface IValidateUserDefinedAddForDisposalBase : IValidateUserDefinedMethod;

internal abstract class ValidateUserDefinedAddForDisposalBase : ValidateUserDefinedMethod, IValidateUserDefinedAddForDisposalBase
{
    internal ValidateUserDefinedAddForDisposalBase(
        ILocalDiagLogger localDiagLogger) 
        : base(localDiagLogger)
    {
    }
    protected abstract INamedTypeSymbol? DisposableType { get; }

    public override void Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        base.Validate(method, rangeType, containerType);

        if (method.DeclaredAccessibility != Accessibility.Private && method.DeclaredAccessibility != Accessibility.Protected)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be private or protected."),
                method.Locations.FirstOrDefault() ?? Location.None);

        if (!method.ReturnsVoid)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a return type."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (!method.IsPartialDefinition)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "User-defined part has to have the partial definition only."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (DisposableType is null)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Disposal type \"System.IAsyncDisposable\" isn't available."),
                method.Locations.FirstOrDefault() ?? Location.None);
        else if (method.Parameters.Length != 1 || !CustomSymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, DisposableType))
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, $"Has to have a single parameter which is of type \"{DisposableType.FullName()}\"."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (method.MethodKind != MethodKind.Ordinary)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be an ordinary method."),
                method.Locations.FirstOrDefault() ?? Location.None);
        
        if (!method.CanBeReferencedByName)
            LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Should be able to be referenced by name."),
                method.Locations.FirstOrDefault() ?? Location.None);

        if (method.Parameters.Length == 1 && method.Parameters[0] is {} parameter)
        {
            if (parameter.IsDiscard)
                LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be discard parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
            
            if (parameter.IsOptional)
                LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be optional."),
                method.Locations.FirstOrDefault() ?? Location.None);
            
            if (parameter.IsParams)
                LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be params parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
            
            if (parameter.IsThis)
                LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be this parameter."),
                method.Locations.FirstOrDefault() ?? Location.None);
            
            if (parameter.RefKind != RefKind.None)
                LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be of a special ref kind."),
                method.Locations.FirstOrDefault() ?? Location.None);
            
            if (parameter.HasExplicitDefaultValue)
                LocalDiagLogger.Error(
                ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to have explicit default value."),
                method.Locations.FirstOrDefault() ?? Location.None);
        }
    }
}