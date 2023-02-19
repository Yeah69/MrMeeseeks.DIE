using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Validation.Range.UserDefined;

internal interface IValidateUserDefinedAddForDisposalSync : IValidateUserDefinedAddForDisposalBase
{
    
}

internal class ValidateUserDefinedAddForDisposalSync : ValidateUserDefinedAddForDisposalBase,
    IValidateUserDefinedAddForDisposalSync
{

    public ValidateUserDefinedAddForDisposalSync(WellKnownTypes wellKnownTypes) => 
        DisposableType = wellKnownTypes.IDisposable;

    protected override INamedTypeSymbol DisposableType { get; }
}

internal interface IValidateUserDefinedAddForDisposalAsync : IValidateUserDefinedAddForDisposalBase
{
    
}

internal class ValidateUserDefinedAddForDisposalAsync : ValidateUserDefinedAddForDisposalBase,
    IValidateUserDefinedAddForDisposalAsync
{

    public ValidateUserDefinedAddForDisposalAsync(WellKnownTypes wellKnownTypes) => 
        DisposableType = wellKnownTypes.IAsyncDisposable;

    protected override INamedTypeSymbol DisposableType { get; }
}

internal interface IValidateUserDefinedAddForDisposalBase : IValidateUserDefinedMethod
{
    
}

internal abstract class ValidateUserDefinedAddForDisposalBase : ValidateUserDefinedMethod, IValidateUserDefinedAddForDisposalBase
{
    protected abstract INamedTypeSymbol DisposableType { get; }

    public override IEnumerable<Diagnostic> Validate(IMethodSymbol method, INamedTypeSymbol rangeType, INamedTypeSymbol containerType)
    {
        foreach (var diagnostic in base.Validate(method, rangeType, containerType))
            yield return diagnostic;

        if (method is
            {
                ReturnsVoid: true,
                IsPartialDefinition: true,
                Parameters.Length: 1,
                MethodKind: MethodKind.Ordinary,
                CanBeReferencedByName: true
            }
            && method.Parameters[0] is
            {
                IsDiscard: false,
                IsOptional: false,
                IsParams: false,
                IsThis: false,
                RefKind: RefKind.None,
                HasExplicitDefaultValue: false
            }
            && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, DisposableType))
        {
        }

        if (!method.ReturnsVoid)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Isn't allowed to have a return type.");
        
        if (!method.IsPartialDefinition)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "User-defined part has to have the partial definition only.");
        
        if (method.Parameters.Length != 1 || !SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type.OriginalDefinition, DisposableType.OriginalDefinition))
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, $"Has to have a single parameter which is of type \"{DisposableType.FullName()}\".");
        
        if (method.MethodKind != MethodKind.Ordinary)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Has to be an ordinary method.");
        
        if (!method.CanBeReferencedByName)
            yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Should be able to be referenced by name.");

        if (method.Parameters.Length == 1 && method.Parameters[0] is {} parameter)
        {
            if (parameter.IsDiscard)
                yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be discard parameter.");
            
            if (parameter.IsOptional)
                yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be optional.");
            
            if (parameter.IsParams)
                yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be params parameter.");
            
            if (parameter.IsThis)
                yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be this parameter.");
            
            if (parameter.RefKind != RefKind.None)
                yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to be of a special ref kind.");
            
            if (parameter.HasExplicitDefaultValue)
                yield return ValidationErrorDiagnostic(method, rangeType, containerType, "Parameter isn't allowed to have explicit default value.");
        }
    }
}