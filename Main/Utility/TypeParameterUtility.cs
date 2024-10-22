using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal interface ITypeParameterUtility
{
    ITypeSymbol ReplaceTypeParametersByCustom(ITypeSymbol baseType);
    IReadOnlyList<ITypeParameterSymbol> ExtractTypeParameters(ITypeSymbol baseType);
    bool CheckLegitimacyOfTypeArguments(INamedTypeSymbol type);
    bool CheckAssignability(ITypeParameterSymbol subject, ITypeParameterSymbol target);
    bool ContainsOpenTypeParameters(ITypeSymbol type);
    ITypeSymbol EquipWithMappedTypeParameters(ITypeSymbol type);
}

internal sealed class TypeParameterUtility : ITypeParameterUtility, IContainerInstance
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<string, IReadOnlyDictionary<ITypeParameterSymbol, string>, IGrownTypeParameterConstraintsDisplayer> _grownTypeParameterConstraintsDisplayerFactory;
    private readonly Compilation _compilation;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Lazy<Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>> _typeParameterToContainerTypeParameter;

    internal TypeParameterUtility(
        Lazy<IContainerNode> parentContainer,
        IReferenceGenerator referenceGenerator,
        GeneratorExecutionContext generatorExecutionContext,
        Func<string, IReadOnlyDictionary<ITypeParameterSymbol, string>, IGrownTypeParameterConstraintsDisplayer> grownTypeParameterConstraintsDisplayerFactory,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        _referenceGenerator = referenceGenerator;
        _grownTypeParameterConstraintsDisplayerFactory = grownTypeParameterConstraintsDisplayerFactory;
        _compilation = generatorExecutionContext.Compilation;
        _wellKnownTypes = wellKnownTypes;

        _typeParameterToContainerTypeParameter = parentContainer
            .Select(pc => pc.TypeParameters
                .Where(tp => tp.GetAttributes().Any(a =>
                    CustomSymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        wellKnownTypesMiscellaneous.GenericParameterMappingAttribute)))
                .SelectMany(tp => tp
                    .GetAttributes()
                    .Where(a => CustomSymbolEqualityComparer.Default.Equals(
                        a.AttributeClass, 
                        wellKnownTypesMiscellaneous.GenericParameterMappingAttribute))
                    .Select(a =>
                    {
                        var fromType = 
                            (a.ConstructorArguments[0].Value as INamedTypeSymbol)?.OriginalDefinition
                            ?? throw new ArgumentException("The first argument of the attribute must be a type.");
                        var fromTypeParameterName = 
                            a.ConstructorArguments[1].Value
                            ?? throw new ArgumentException("The second argument of the attribute must be a string.");
                        var fromTypeParameter = 
                            fromType.TypeParameters.FirstOrDefault(tp => tp.Name == fromTypeParameterName.ToString())
                            ?? throw new ArgumentException("The second argument of the attribute must be a type parameter of the first argument.");
                        return (From: fromTypeParameter, To: tp);
                    }))
                .ToDictionary(t => t.From, t => t.To));
    }

    public ITypeSymbol ReplaceTypeParametersByCustom(ITypeSymbol baseType)
    {
        var typeParametersMap = GrowFor(baseType);

        if (typeParametersMap.Count == 0) return baseType;

        var (_, newType) = Inner(baseType);

        return newType;

        (bool ReplacementOccured, ITypeSymbol Type) Inner(ITypeSymbol type)
        {
            switch (type)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                    var (replaced, newType) = Inner(arrayTypeSymbol.ElementType);
                    return replaced
                        ? (true, _compilation.CreateArrayTypeSymbol(newType))
                        : (false, type);
                case IDynamicTypeSymbol:
                    throw new ArgumentException("Dynamic can't be used as type constraint.", nameof(type));
                case IErrorTypeSymbol:
                    throw new ArgumentException("Error can't be used as type constraint.", nameof(type));
                case IFunctionPointerTypeSymbol:
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
                    throw new ArgumentException(
                        "Pointer type can't be used as type constraint, directly (see 'unmanaged' constraint).",
                        nameof(type));
                case ITypeParameterSymbol typeParameterSymbol:
                    return _typeParameterToContainerTypeParameter.Value.ContainsValue(typeParameterSymbol) 
                        ? (false, typeParameterSymbol) 
                        // This type parameter has to be contained in the dictionary, otherwise the type extraction failed
                        : (true, typeParametersMap[typeParameterSymbol]);

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
                    var originalTypeParameter = typeParameterSymbol.OriginalDefinition;
                    if (_typeParameterToContainerTypeParameter.Value.ContainsValue(originalTypeParameter))
                        break;
                    collectedTypeParameterSymbols.Add(typeParameterSymbol);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }

    private Dictionary<ITypeParameterSymbol, ITypeParameterSymbol> GrowFor(ITypeSymbol baseType)
    {
        var extractedOpenTypeParameters = ExtractTypeParameters(baseType);

        if (!extractedOpenTypeParameters.Any()) return new Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>();

        var extractedToGrownNames = extractedOpenTypeParameters
            .ToDictionary(e => e, _ => _referenceGenerator.Generate("T"));

        var constraintsList = string.Join(
            Environment.NewLine,
            extractedToGrownNames.Select(kvp =>
                _grownTypeParameterConstraintsDisplayerFactory(kvp.Value, extractedToGrownNames).Display(kvp.Key)));
        
        var surrogateCode =
            $$"""
              namespace N
              {
                  public class C<{{string.Join(", ", extractedToGrownNames.Values)}}> {{constraintsList}}
                  {}
              }
              """;

        var languageVersion = (_compilation as CSharpCompilation)?.LanguageVersion ?? LanguageVersion.Default;
        var syntaxTree =
            CSharpSyntaxTree.ParseText(surrogateCode, new CSharpParseOptions(languageVersion: languageVersion));

        var newCompilation = _compilation.AddSyntaxTrees(syntaxTree);

        var newType = newCompilation.GetTypeByMetadataName($"N.C`{extractedToGrownNames.Count}") ??
                      throw new ImpossibleDieException("Impossible: Didn't found type for type parameter growing.");
        return extractedOpenTypeParameters
            .Zip(newType.TypeParameters, (l, r) => (l, r))
            .ToDictionary(t => t.l, t => t.r);
    }

    public bool CheckLegitimacyOfTypeArguments(INamedTypeSymbol type)
    {
        var originalImplementation = type.OriginalDefinition;
        var originalTypeParameters = originalImplementation.TypeParameters;
        var typeArguments = type.TypeArguments;
        
        var originalTypeParametersToOriginalTypeArguments = originalTypeParameters
            .Zip(originalImplementation.TypeArguments, (l, r) => (l, r))
            .ToDictionary(t => t.l, t => t.r);

        for (int i = 0; i < typeArguments.Length; i++)
        {
            var currentTypeArgument = typeArguments[i];
            var currentTypeParameter = originalTypeParameters[i];
            if (!CheckLegitimacyOfSingleTypeArgument(currentTypeParameter, currentTypeArgument, originalTypeParametersToOriginalTypeArguments))
                return false;
        }

        return true;
    }

    public bool CheckAssignability(ITypeParameterSymbol subject, ITypeParameterSymbol target) =>
        CheckLegitimacyOfSingleTypeArgument(target, subject, new Dictionary<ITypeParameterSymbol, ITypeSymbol>());

    private bool CheckLegitimacyOfSingleTypeArgument(
        ITypeParameterSymbol currentTypeParameter,
        ITypeSymbol currentTypeArgument, 
        Dictionary<ITypeParameterSymbol, ITypeSymbol> originalTypeParametersToOriginalTypeArguments)
    {
        if (currentTypeParameter.HasValueTypeConstraint)
        {
            switch (currentTypeArgument)
            {
                case INamedTypeSymbol namedTypeSymbol:
                    if (!namedTypeSymbol.IsValueType)
                        return false;
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    if (!typeParameterSymbol.HasValueTypeConstraint)
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (currentTypeParameter.HasReferenceTypeConstraint)
        {
            switch (currentTypeArgument)
            {
                case IArrayTypeSymbol:
                    // All arrays are reference types
                    break;
                case INamedTypeSymbol namedTypeSymbol:
                    if (!namedTypeSymbol.IsReferenceType)
                        return false;
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    if (!typeParameterSymbol.HasReferenceTypeConstraint)
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (currentTypeParameter.HasNotNullConstraint)
        {
            switch (currentTypeArgument)
            {
                case IArrayTypeSymbol arrayTypeSymbol:
                    if (arrayTypeSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated)
                        return false;
                    break;
                case INamedTypeSymbol namedTypeSymbol:
                    if (namedTypeSymbol is
                            { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated }
                        && !CustomSymbolEqualityComparer.Default.Equals(namedTypeSymbol.UnboundIfGeneric(),
                            _wellKnownTypes.Nullable1.UnboundIfGeneric()))
                        return false;
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    if (!typeParameterSymbol.HasNotNullConstraint)
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (currentTypeParameter.HasUnmanagedTypeConstraint)
        {
            switch (currentTypeArgument)
            {
                case INamedTypeSymbol namedTypeSymbol:
                    if (!namedTypeSymbol.IsUnmanagedType)
                        return false;
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    if (!typeParameterSymbol.HasUnmanagedTypeConstraint)
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (currentTypeParameter.HasConstructorConstraint)
        {
            switch (currentTypeArgument)
            {
                case INamedTypeSymbol namedTypeSymbol:
                    if (namedTypeSymbol.IsValueType
                        || namedTypeSymbol.InstanceConstructors.Any(c => c is
                            { Parameters.Length: 0, DeclaredAccessibility: Accessibility.Public }))
                        return false;
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    if (!typeParameterSymbol.HasConstructorConstraint)
                        return false;
                    break;
                default:
                    return false;
            }
        }

        for (int j = 0; j < currentTypeParameter.ConstraintTypes.Length; j++)
        {
            var constraintType = currentTypeParameter.ConstraintTypes[j];
            var nullableAnnotation = currentTypeParameter.ConstraintNullableAnnotations[j];
            switch (currentTypeArgument)
            {
                case INamedTypeSymbol argumentNamedTypeSymbol:
                    if (nullableAnnotation != NullableAnnotation.None &&
                        argumentNamedTypeSymbol.NullableAnnotation != nullableAnnotation)
                        return false;
                    if (constraintType is not INamedTypeSymbol constraintNamedTypeSymbol)
                        return false;

                    if (!argumentNamedTypeSymbol.AllDerivedTypesAndSelf()
                        .Any(t => Fit(constraintNamedTypeSymbol, t)))
                        return false;

                    break;
                case ITypeParameterSymbol argumentTypeParameterSymbol:
                    if (!argumentTypeParameterSymbol.ConstraintTypes.Any(ct => Fit(constraintType, ct)))
                        return false;
                    break;
                default:
                    return false;
            }

            continue;

            bool Fit(ITypeSymbol constraint, ITypeSymbol argument)
            {
                switch (constraint)
                {
                    case IArrayTypeSymbol constraintArray:
                        if (argument is not IArrayTypeSymbol argumentArray)
                            return false;
                        return Fit(constraintArray.ElementType, argumentArray.ElementType);
                    case IDynamicTypeSymbol:
                        return argument is IDynamicTypeSymbol;
                    case IErrorTypeSymbol:
                        return false;
                    case IFunctionPointerTypeSymbol constraintFunctionPointer:
                        if (argument is not IFunctionPointerTypeSymbol argumentFunctionPointer)
                            return false;
                        if (constraintFunctionPointer.Signature.ReturnsVoid != argumentFunctionPointer.Signature.ReturnsVoid
                            || !Fit(constraintFunctionPointer.Signature.ReturnType, argumentFunctionPointer.Signature.ReturnType))
                            return false;
                        return constraintFunctionPointer.Signature.Parameters.Length == argumentFunctionPointer.Signature.Parameters.Length
                               && constraintFunctionPointer.Signature.Parameters
                                   .Select((t, l) => Fit(t.Type, argumentFunctionPointer.Signature.Parameters[l].Type))
                                   .All(b => b);
                    case INamedTypeSymbol constraintNamed:
                        if (argument is not INamedTypeSymbol argumentNamed)
                            return false;
                        if (constraintNamed.Arity != argumentNamed.Arity 
                            || FullyQualifiedName(constraintNamed) != FullyQualifiedName(argumentNamed))
                            return false;

                        for (int k = 0; k < constraintNamed.TypeArguments.Length; k++)
                        {
                            var constraintTypeArgument = constraintNamed.TypeArguments[k];
                            var argumentTypeArgument = argumentNamed.TypeArguments[k];
                            if (!Fit(constraintTypeArgument, argumentTypeArgument))
                                return false;
                        }

                        return true;

                        string FullyQualifiedName(INamedTypeSymbol type) =>
                            type.ToDisplayString(new SymbolDisplayFormat(
                                SymbolDisplayGlobalNamespaceStyle.Included, 
                                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                                SymbolDisplayGenericsOptions.None, 
                                SymbolDisplayMemberOptions.IncludeRef, 
                                parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType, 
                                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None));
                    case IPointerTypeSymbol constraintPointer:
                        if (argument is not IPointerTypeSymbol argumentPointer)
                            return false;
                        return Fit(constraintPointer.PointedAtType, argumentPointer.PointedAtType);
                    case ITypeParameterSymbol constraintTypeParameter:
                        if (!originalTypeParametersToOriginalTypeArguments.TryGetValue(constraintTypeParameter, out var constraintNext))
                            return false;
                        if (constraintNext is ITypeParameterSymbol constraintNextTypeParameter)
                            return CheckLegitimacyOfSingleTypeArgument(constraintNextTypeParameter, argument, originalTypeParametersToOriginalTypeArguments);
                        return Fit(constraintNext, argument);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(constraint));
                }
            }
        }

        return true;
    }

    public bool ContainsOpenTypeParameters(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol arrayTypeSymbol:
                return ContainsOpenTypeParameters(arrayTypeSymbol.ElementType);
            case IDynamicTypeSymbol:
                return false;
            case IErrorTypeSymbol:
                return false;
            case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                return functionPointerTypeSymbol.Signature.Parameters.Any(p => ContainsOpenTypeParameters(p.Type))
                    || functionPointerTypeSymbol.Signature.TypeArguments.Any(ContainsOpenTypeParameters);
            case INamedTypeSymbol namedTypeSymbol:
                return namedTypeSymbol.TypeArguments.Any(ContainsOpenTypeParameters);
            case IPointerTypeSymbol pointerTypeSymbol:
                return ContainsOpenTypeParameters(pointerTypeSymbol.PointedAtType);
            case ITypeParameterSymbol typeParameterSymbol:
                return !_typeParameterToContainerTypeParameter.Value.ContainsValue(typeParameterSymbol);
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    public ITypeSymbol EquipWithMappedTypeParameters(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol {IsUnboundGenericType: true} unbound) return type;

        var original = unbound.OriginalDefinition;
        
        var mapUsed = false;

        var newTypeParameters = original.TypeParameters.Select(tp =>
        {
            if (_typeParameterToContainerTypeParameter.Value.TryGetValue(tp, out var mappedTypeParameter))
            {
                mapUsed = true;
                return mappedTypeParameter;
            }

            return tp;
        }).OfType<ITypeSymbol>().ToArray();
        
        return mapUsed ? original.Construct(newTypeParameters) : type;
    }
}