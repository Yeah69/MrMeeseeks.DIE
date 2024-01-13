using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Utility;

internal interface ITypeParameterUtility
{
    ITypeSymbol ReplaceTypeParametersByCustom(ITypeSymbol baseType);
    IReadOnlyList<ITypeParameterSymbol> ExtractTypeParameters(ITypeSymbol baseType);
}

internal class TypeParameterUtility : ITypeParameterUtility
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Compilation _compilation;

    internal TypeParameterUtility(
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext)
    {
        _referenceGenerator = referenceGenerator;
        _compilation = containerWideContext.Compilation;
    }

    public ITypeSymbol ReplaceTypeParametersByCustom(ITypeSymbol baseType)
    {
        var typeParametersMap = GrowFor(baseType);

        if (!typeParametersMap.Any()) return baseType;

        var (_, newType) = Inner(baseType);

        return newType;

        (bool ReplacementOccured, ITypeSymbol Type) Inner(ITypeSymbol type)
        {
            switch (type)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                    var (replaced, newType) = Inner(arrayTypeSymbol.ElementType);
                    return replaced 
                        ? (true, _compilation.CreateArrayTypeSymbol(newType)) : 
                        (false, type);
                case IDynamicTypeSymbol:
                    throw new ArgumentException("Dynamic can't be used as type constraint.", nameof(type));
                case IErrorTypeSymbol:
                    throw new ArgumentException("Error can't be used as type constraint.", nameof(type));
                case IFunctionPointerTypeSymbol :
                    throw new ArgumentException("Function pointer can't be used as type constraint.", nameof(type));
                case INamedTypeSymbol namedTypeSymbol:
                    var newTypeArguments = namedTypeSymbol.TypeArguments.Select(Inner).ToList();
                    if (newTypeArguments.Any(t => t.ReplacementOccured))
                    {
                        var newNamedTypeSymbol =
                            namedTypeSymbol.OriginalDefinition.Construct(newTypeArguments.Select(t => t.Type)
                                .ToArray());
                        return (true, newNamedTypeSymbol);
                    }

                    return (false, type);
                case IPointerTypeSymbol:
                    throw new ArgumentException("Pointer type can't be used as type constraint, directly (see 'unmanaged' constraint).", nameof(type));
                case ITypeParameterSymbol typeParameterSymbol:
                    // This type parameter has to be contained in the dictionary, otherwise the type extraction failed
                    return (true, typeParametersMap[typeParameterSymbol]);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }

    public IReadOnlyList<ITypeParameterSymbol> ExtractTypeParameters(ITypeSymbol baseType)
    {
        var collectedTypeParameterSymbols = new HashSet<ITypeParameterSymbol>(CustomSymbolEqualityComparer.Default);
        
        Inner(baseType);

        return collectedTypeParameterSymbols.ToList();

        void Inner(ITypeSymbol type)
        {
            switch (type)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                    Inner(arrayTypeSymbol.ElementType);
                    break;
                case IDynamicTypeSymbol:
                    return;
                case IErrorTypeSymbol:
                    return;
                case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                    if (!functionPointerTypeSymbol.Signature.ReturnsVoid)
                        Inner(functionPointerTypeSymbol.Signature.ReturnType);
                    foreach (var signatureParameter in functionPointerTypeSymbol.Signature.Parameters)
                    {
                        Inner(signatureParameter.Type);
                    }
                    foreach (var signatureTypeArgument in functionPointerTypeSymbol.Signature.TypeArguments)
                    {
                        Inner(signatureTypeArgument);
                    }
                    break;
                case INamedTypeSymbol namedTypeSymbol:
                    foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                    {
                        Inner(typeArgument);
                    }
                    break;
                case IPointerTypeSymbol pointerTypeSymbol:
                    Inner(pointerTypeSymbol.PointedAtType);
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    collectedTypeParameterSymbols.Add(typeParameterSymbol);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }

    public IReadOnlyDictionary<ITypeParameterSymbol, ITypeParameterSymbol> GrowFor(ITypeSymbol baseType)
    {
        var extractedOpenTypeParameters = ExtractTypeParameters(baseType);

        if (!extractedOpenTypeParameters.Any()) return new Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>();

        var extractedToGrownNames = extractedOpenTypeParameters
            .ToDictionary(e => e, _ => _referenceGenerator.Generate("T"));
        
        var surrogateCode =
            $$"""
              namespace N
              {
                  public class C<{{string.Join(", ", extractedToGrownNames.Values)}}> {{string.Join(Environment.NewLine, extractedToGrownNames.Select(kvp => TypeParameterToConstraintsString(kvp.Key, kvp.Value)))}}
                  {}
              }
              """;

        var languageVersion = (_compilation as CSharpCompilation)?.LanguageVersion ?? LanguageVersion.Default;
        var syntaxTree = CSharpSyntaxTree.ParseText(surrogateCode, new CSharpParseOptions(languageVersion: languageVersion));

        var newCompilation = _compilation.AddSyntaxTrees(syntaxTree);

        var newType = newCompilation.GetTypeByMetadataName($"N.C`{extractedToGrownNames.Count}") ?? throw new Exception("Impossible: Didn't found type for type parameter growing.");
        return extractedOpenTypeParameters
            .Zip(newType.TypeParameters, (l, r) => (l, r))
            .ToDictionary(t => t.l, t => t.r);

        string TypeParameterToConstraintsString(ITypeParameterSymbol tp, string name)
        {
            var constraints = new List<string>();
            if (tp.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");
            else if (tp.HasValueTypeConstraint)
                constraints.Add("struct");
            else if (tp.HasReferenceTypeConstraint)
                constraints.Add($"class{(tp.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "?" : "")}");
            else if (tp.HasNotNullConstraint)
                constraints.Add("notnull");
            if (tp.HasConstructorConstraint)
                constraints.Add("new()");
            constraints.AddRange(tp.ConstraintTypes.Select(GetTypeConstraintString));
            return constraints.Any() ? $" where {name} : {string.Join(", ", constraints)}" : "";

            string GetTypeConstraintString(ITypeSymbol t)
            {
                switch (t)
                {
                    case IArrayTypeSymbol arrayTypeSymbol:
                        // Arrays can't be type constraints top-level, but the can be nested in the type parameters
                        return $"{GetTypeConstraintString(arrayTypeSymbol.ElementType)}[]";
                    case IDynamicTypeSymbol:
                        throw new ArgumentException("Dynamic can't be used as type constraint.", nameof(t));
                    case IErrorTypeSymbol:
                        throw new ArgumentException("Error can't be used as type constraint.", nameof(t));
                    case IFunctionPointerTypeSymbol :
                        throw new ArgumentException("Function pointer can't be used as type constraint.", nameof(t));
                    case INamedTypeSymbol namedTypeSymbol:
                        var withoutTypeParameters = namedTypeSymbol.ToDisplayString(new SymbolDisplayFormat(
                            SymbolDisplayGlobalNamespaceStyle.Included,
                            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                            SymbolDisplayGenericsOptions.None, 
                            SymbolDisplayMemberOptions.IncludeRef,
                            parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut |
                                              SymbolDisplayParameterOptions.IncludeType,
                            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
                        var typeParameters = namedTypeSymbol.TypeArguments.Any()
                            ? $"<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(GetTypeConstraintString))}>"
                            : "";
                        return $"{withoutTypeParameters}{typeParameters}";
                    case IPointerTypeSymbol:
                        throw new ArgumentException("Pointer type can't be used as type constraint, directly (see 'unmanaged' constraint).", nameof(t));
                    case ITypeParameterSymbol typeParameterSymbol:
                        // This type parameter has to be contained in the dictionary, otherwise the type extraction failed
                        return extractedToGrownNames[typeParameterSymbol];
                    default:
                        throw new ArgumentOutOfRangeException(nameof(t));
                }
            }
        }
    }
}