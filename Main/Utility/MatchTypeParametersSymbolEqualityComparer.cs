using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

public sealed class MatchTypeParametersSymbolEqualityComparer : IEqualityComparer<ISymbol?>
{
    private readonly bool _considerNullability;
    private readonly SymbolEqualityComparer _originalSymbolEqualityComparer;
    
    public static readonly MatchTypeParametersSymbolEqualityComparer Default = new (false);
    public static readonly MatchTypeParametersSymbolEqualityComparer IncludeNullability = new (true);

    internal MatchTypeParametersSymbolEqualityComparer(bool considerNullability)
    {
        _considerNullability = considerNullability;
        _originalSymbolEqualityComparer = considerNullability
            ? SymbolEqualityComparer.IncludeNullability
            : SymbolEqualityComparer.Default;
    }

    public bool Equals(ISymbol? x, ISymbol? y)
    {
        if (x is not ITypeSymbol xType || y is not ITypeSymbol yType) 
            return _originalSymbolEqualityComparer.Equals(x, y);
        
        var map = new Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>();
        return CheckTypeArgument(xType, yType);

        bool CheckTypeArgument(ITypeSymbol xCurrent, ITypeSymbol yCurrent)
        {
            switch (xCurrent)
            {
                case IArrayTypeSymbol xArray when yCurrent is IArrayTypeSymbol yArray:
                    return xArray.ElementNullableAnnotation == yArray.ElementNullableAnnotation
                        && Equals(xArray.ElementType, yArray.ElementType);
                case IDynamicTypeSymbol when yCurrent is IDynamicTypeSymbol:
                    return false;
                case IErrorTypeSymbol when yCurrent is IErrorTypeSymbol:
                    return false;
                case IFunctionPointerTypeSymbol xFunctionPointer when yType is IFunctionPointerTypeSymbol yFunctionPointer:
                    // This case probably won't be relevant
                    return _originalSymbolEqualityComparer.Equals(xFunctionPointer, yFunctionPointer);
                case INamedTypeSymbol xNamed when yCurrent is INamedTypeSymbol yNamed:
                    return _originalSymbolEqualityComparer.Equals(xNamed.ContainingNamespace,
                               yNamed.ContainingNamespace)
                           && xNamed.Name == yNamed.Name
                           && xNamed.TypeArguments.Length == yNamed.TypeArguments.Length
                           && xNamed.TypeArguments
                               .Zip(yNamed.TypeArguments, CheckTypeArgument).All(b => b)
                           && (!_considerNullability
                               // either both annotated
                               || xNamed.NullableAnnotation == NullableAnnotation.Annotated &&
                               yNamed.NullableAnnotation == NullableAnnotation.Annotated
                               // or both not annotated
                               || xNamed.NullableAnnotation != NullableAnnotation.Annotated &&
                               yNamed.NullableAnnotation != NullableAnnotation.Annotated);
                case IPointerTypeSymbol xPointer when yCurrent is IPointerTypeSymbol yPointer:
                    return Equals(xPointer.PointedAtType, yPointer.PointedAtType);
                case ITypeParameterSymbol xTypeParam when yCurrent is ITypeParameterSymbol yTypeParam:
                {
                    if (map.TryGetValue(xTypeParam, out var fountYTypeParam))
                    {
                        return fountYTypeParam == yTypeParam;
                    }
                    // Ignore Name, ContainingType and so on
                    var areEqual = xTypeParam.HasReferenceTypeConstraint == yTypeParam.HasReferenceTypeConstraint
                                   && xTypeParam.ReferenceTypeConstraintNullableAnnotation == yTypeParam.ReferenceTypeConstraintNullableAnnotation
                                   && xTypeParam.HasConstructorConstraint == yTypeParam.HasConstructorConstraint
                                   && xTypeParam.HasNotNullConstraint == yTypeParam.HasNotNullConstraint
                                   && xTypeParam.HasUnmanagedTypeConstraint == yTypeParam.HasUnmanagedTypeConstraint
                                   && xTypeParam.HasValueTypeConstraint == yTypeParam.HasValueTypeConstraint
                                   && xTypeParam.ConstraintTypes.Length == yTypeParam.ConstraintTypes.Length
                                   && xTypeParam.ConstraintTypes.Zip(yTypeParam.ConstraintTypes, CheckTypeArgument).All(b => b)
                                   && xTypeParam.ConstraintNullableAnnotations.Zip(yTypeParam.ConstraintNullableAnnotations, (l, r) => (l, r))
                                       .All(t => t.l == t.r);
                    if (areEqual)
                    {
                        map[xTypeParam] = yTypeParam;
                    }

                    return areEqual;
                }

                default:
                    return false;
            }
        }
    }

    public int GetHashCode(ISymbol? obj)
    {
        return obj is INamedTypeSymbol named
            ? ForNamed(named)
            : _originalSymbolEqualityComparer.GetHashCode(obj);

        static int ForNamed(INamedTypeSymbol named)
        {
            return Inner(named).GetHashCode();

            string Inner(ITypeSymbol inner)
            {
                switch (inner)
                {
                    case IArrayTypeSymbol arrayTypeSymbol:
                        return $"{Inner(arrayTypeSymbol.ElementType)}[]";
                    case IDynamicTypeSymbol:
                        return "dynamic";
                    case IErrorTypeSymbol errorTypeSymbol:
                        return errorTypeSymbol.FullName();
                    case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                        return $"delegate*<{string.Join(",", functionPointerTypeSymbol.Signature.Parameters.Select(ps => Inner(ps.Type)))},{
                                    (functionPointerTypeSymbol.Signature.ReturnsVoid ? "void" : Inner(functionPointerTypeSymbol.Signature.ReturnType))}>";
                    case INamedTypeSymbol namedTypeSymbol:
                        var withoutTypeParameters = 
                            namedTypeSymbol.ToDisplayString(SymbolDisplayFormatPicks.FullNameExceptTypeParameters);
                        return $"{withoutTypeParameters}<{string.Join(",", namedTypeSymbol.TypeArguments.Select(Inner))}>";
                    case IPointerTypeSymbol pointerTypeSymbol:
                        return $"{Inner(pointerTypeSymbol.PointedAtType)}*";
                    case ITypeParameterSymbol:
                        // Ignore type parameters. In doubt Equals can filter more deliberately.
                        return "";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inner));
                }
            }
        }
    }
}