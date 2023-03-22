namespace MrMeeseeks.DIE.Validation.Attributes;

internal interface IValidateAttributes
{
    bool ValidateAbstraction(INamedTypeSymbol type);
    bool ValidateImplementation(INamedTypeSymbol type);
}

internal class ValidateAttributes : IValidateAttributes
{
    public bool ValidateAbstraction(INamedTypeSymbol type) => 
        type.TypeKind == TypeKind.Interface || type.IsReferenceType;

    public bool ValidateImplementation(INamedTypeSymbol type) =>
        type.IsValueType
        || type is { IsReferenceType: true, IsAbstract: false, TypeKind: not TypeKind.Interface };
}