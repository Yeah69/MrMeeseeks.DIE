namespace MrMeeseeks.DIE.Utility;


internal abstract class TypeParameterConstraintsDisplayBase
{
    protected abstract string GetName(ITypeParameterSymbol typeParameter);
    
    protected abstract string GetNameOfNestedTypeParameter(ITypeParameterSymbol typeParameter);
    
    public string Display(ITypeParameterSymbol typeParameter)
    {
        var constraints = new List<string>();
            if (typeParameter.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");
            else if (typeParameter.HasValueTypeConstraint)
                constraints.Add("struct");
            else if (typeParameter.HasReferenceTypeConstraint)
                constraints.Add(
                    $"class{(typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "?" : "")}");
            if (typeParameter.HasNotNullConstraint)
                constraints.Add("notnull");
            constraints.AddRange(typeParameter.ConstraintTypes.Select(GetTypeConstraintString));
            if (typeParameter.HasConstructorConstraint)
                constraints.Add("new()");
            return constraints.Count > 0 ? $" where {GetName(typeParameter)} : {string.Join(", ", constraints)}" : "";

            string GetTypeConstraintString(ITypeSymbol t)
            {
                switch (t)
                {
                    case IArrayTypeSymbol arrayTypeSymbol:
                        // Arrays can't be type constraints top-level, but can be nested in the type parameters
                        return $"{GetTypeConstraintString(arrayTypeSymbol.ElementType)}[]";
                    case IDynamicTypeSymbol:
                        throw new ArgumentException("Dynamic can't be used as type constraint.", nameof(t));
                    case IErrorTypeSymbol:
                        throw new ArgumentException("Error can't be used as type constraint.", nameof(t));
                    case IFunctionPointerTypeSymbol:
                        throw new ArgumentException("Function pointer can't be used as type constraint.", nameof(t));
                    case INamedTypeSymbol namedTypeSymbol:
                        var withoutTypeParameters = 
                            namedTypeSymbol.ToDisplayString(SymbolDisplayFormatPicks.FullNameExceptTypeParameters);
                        var typeParameters = namedTypeSymbol.TypeArguments.Any()
                            ? $"<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(GetTypeConstraintString))}>"
                            : "";
                        return $"{withoutTypeParameters}{typeParameters}";
                    case IPointerTypeSymbol:
                        throw new ArgumentException(
                            "Pointer type can't be used as type constraint, directly (see 'unmanaged' constraint).",
                            nameof(t));
                    case ITypeParameterSymbol typeParameterSymbol:
                        // This type parameter has to be contained in the dictionary, otherwise the type extraction failed
                        return GetNameOfNestedTypeParameter(typeParameterSymbol);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(t));
                }
            }
    }
}

internal interface IGrownTypeParameterConstraintsDisplayer
{
    string Display(ITypeParameterSymbol typeParameter);
}

internal sealed class GrownTypeParameterConstraintsDisplayer : TypeParameterConstraintsDisplayBase, IGrownTypeParameterConstraintsDisplayer
{
    private readonly string _name;
    private readonly IReadOnlyDictionary<ITypeParameterSymbol,string> _extractedToGrownNames;

    internal GrownTypeParameterConstraintsDisplayer(
        string name,
        IReadOnlyDictionary<ITypeParameterSymbol, string> extractedToGrownNames)
    {
        _name = name;
        _extractedToGrownNames = extractedToGrownNames;
    }

    protected override string GetName(ITypeParameterSymbol typeParameter) => _name;

    protected override string GetNameOfNestedTypeParameter(ITypeParameterSymbol typeParameter) 
        => _extractedToGrownNames[typeParameter];
}

internal interface IOrdinaryTypeParameterConstraintsDisplayer
{
    string Display(ITypeParameterSymbol typeParameter);
}

internal sealed class OrdinaryTypeParameterConstraintsDisplayer : TypeParameterConstraintsDisplayBase, IOrdinaryTypeParameterConstraintsDisplayer
{
    protected override string GetName(ITypeParameterSymbol typeParameter) => typeParameter.Name;

    protected override string GetNameOfNestedTypeParameter(ITypeParameterSymbol typeParameter) 
        => typeParameter.Name;
}