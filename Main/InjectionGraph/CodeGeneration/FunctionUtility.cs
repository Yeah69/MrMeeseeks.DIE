using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class FunctionUtility(
    ReferenceGenerator referenceGenerator,
    ContextGenerator contextGenerator,
    WellKnownTypes wellKnownTypes) : IContainerInstance
{
    internal string DoScopedInstanceParameterName { get; } = referenceGenerator.Generate("doScopedInstance");
    internal string DoScopeRootParameterName { get; } = referenceGenerator.Generate("doScopeRoot");
    private readonly Dictionary<IFunction, string> _namesMap = [];
    
    internal string GetName(IFunction function)
    {
        if (!_namesMap.TryGetValue(function, out var name))
        {
            name = function switch
            {
                FunctorEntryFunction functorEntryFunction => referenceGenerator.Generate("CreateEntry", functorEntryFunction.ReturnType),
                ScopeRootFunction scopeRootFunction => referenceGenerator.Generate("CreateRoot", scopeRootFunction.ReturnType),
                ScopedInstanceFunction => ScopedInstanceInterfaceDescription.FunctionName,
                TypeNodeFunction typeNodeFunction => referenceGenerator.Generate("Create", typeNodeFunction.ReturnType),
                _ => throw new ArgumentOutOfRangeException(nameof(function))
            };
            _namesMap[function] = name;
        }
        return name;
    }

    internal string GenerateFunctionCall(IFunction function, bool doScopedInstance, bool doScopeRoot) => $"{GetName(function)}({contextGenerator.ParameterName}, {DoScopedInstanceParameterName}: {(doScopedInstance ? Constants.TrueKeyword : Constants.FalseKeyword)}, {DoScopeRootParameterName}: {(doScopeRoot ? Constants.TrueKeyword : Constants.FalseKeyword)})";

    internal string GenerateHeader(IFunction function)
    {
        var accessibility =
            function is { Accessibility: { } acc, ExplicitInterface: var explicitInterface }
            && explicitInterface.Equals(ExplicitInterfaceDescription.None4.Instance)
                ? $"{SyntaxFacts.GetText(acc)} "
                : "";
        var asyncModifier = function.IsAsync
            ? "async "
            : "";
        var explicitInterfaceFullName = function.ExplicitInterface switch
            {
                ExplicitInterfaceDescription.Generated generated => $"{generated.TypeFullName}.",
                ExplicitInterfaceDescription.KnownType knownType => knownType.Type.FullName(),
                ExplicitInterfaceDescription.None4 => "",
                _ => throw new ArgumentOutOfRangeException("Should be impossible")
            };
        var typeParameters = "";
        var typeParametersConstraints = "";
        if (function.TypeParameters.Length != 0)
        {
            typeParameters = $"<{string.Join(", ", function.TypeParameters.Select(p => p.Name))}>";
            typeParametersConstraints = string.Join("", function
                .TypeParameters
                .Where(p => p.HasValueTypeConstraint 
                            || p.HasReferenceTypeConstraint
                            || p.HasNotNullConstraint 
                            || p.HasUnmanagedTypeConstraint
                            || p.HasConstructorConstraint
                            || p.ConstraintTypes.Length > 0)
                .Select(p =>
                {
                    var constraints = new List<string>();
                    if (p.HasUnmanagedTypeConstraint)
                        constraints.Add("unmanaged");
                    else if (p.HasValueTypeConstraint)
                        constraints.Add("struct");
                    else if (p.HasReferenceTypeConstraint)
                        constraints.Add($"class{(p.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "?" : "")}");
                    if (p.HasNotNullConstraint)
                        constraints.Add("notnull");
                    constraints.AddRange(p.ConstraintTypes.Select((t, i) => t.WithNullableAnnotation(p.ConstraintNullableAnnotations[i]).FullName()));
                    if (p.HasConstructorConstraint)
                        constraints.Add("new()");
                    return $"{Environment.NewLine}where {p.Name} : {string.Join(", ", constraints)}";
                }));
        }

        var parametersText = $"{contextGenerator.FullNameAndParameterName}, {wellKnownTypes.Boolean.FullName()} {DoScopedInstanceParameterName}, {wellKnownTypes.Boolean.FullName()} {DoScopeRootParameterName}";
        var functionName = GetName(function);
        return $"{accessibility}{asyncModifier}{function.ReturnType.FullName()} {explicitInterfaceFullName}{functionName}{typeParameters}({parametersText}){typeParametersConstraints}";
    }
}