using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class FunctorNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteFunctorNode>, IContainerInstance
{
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly ContextGenerator _contextGenerator;
    private readonly OverrideContextManager _overrideContextManager;
    private readonly SharedNameRegistry _sharedNameRegistry;

    internal FunctorNodeCodeGenerator(
        ReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes,
        ContextGenerator contextGenerator,
        OverrideContextManager overrideContextManager,
        SharedNameRegistry sharedNameRegistry)
    {
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
        _contextGenerator = contextGenerator;
        _overrideContextManager = overrideContextManager;
        _sharedNameRegistry = sharedNameRegistry;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteFunctorNode concreteNode, string? reference = null)
    {
        var actualReference = reference ?? _referenceGenerator.Generate(concreteNode.Data.Type);
        var parameterReferences = concreteNode.FunctorParameterTypes.Select(_ => _referenceGenerator.Generate("p")).ToArray();
        var parameterDeclaration = string.Join(", ", parameterReferences);

        if (_overrideContextManager.TryGetContext(concreteNode.FunctorParameterTypes, out var overrideContext)
            && _sharedNameRegistry.TryGetOverrideContextName(overrideContext, out var overrideContextName))
        {
            var outwardFacingTypeIdReference = _referenceGenerator.Generate("oId");
            var initialCaseIdReference = _referenceGenerator.Generate("cId");
            var keyReference = _referenceGenerator.Generate("kId");
            var containerNodeReference = _referenceGenerator.Generate("cnId");
            var transientScopeNodeReference = _referenceGenerator.Generate("tsnId");
            var scopeNodeReference = _referenceGenerator.Generate("snId");
            var scopeNodeNameReference = _referenceGenerator.Generate("snnId");

            code.AppendLine(
                $$"""
                  {{_wellKnownTypes.Int32.FullName()}} {{outwardFacingTypeIdReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}};
                  {{_wellKnownTypes.Int32.FullName()}} {{initialCaseIdReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.CaseNumberPropertyName}};
                  {{_wellKnownTypes.Object.WithNullableAnnotation(NullableAnnotation.Annotated).FullName()}} {{keyReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.KeyPropertyName}};
                  {{_wellKnownTypes.Object.FullName()}} {{containerNodeReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.ContainerNodePropertyName}};
                  {{_wellKnownTypes.Object.FullName()}} {{transientScopeNodeReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.TransientScopeNodePropertyName}};
                  {{_wellKnownTypes.Object.FullName()}} {{scopeNodeReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.ScopeNodePropertyName}};
                  {{_wellKnownTypes.String.FullName()}} {{scopeNodeNameReference}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.ScopeNodeNamePropertyName}};
                  """);

            var overrideParameters = overrideContext is OverrideContext.Any any
                ? string.Join(", ", any.Overrides.Select(o => parameterReferences[concreteNode.FunctorParameterTypes.Select((t, i) => (t, i)).First(t => CustomSymbolEqualityComparer.IncludeNullability.Equals(t.t, o)).i]))
                : "";

            var parameters = string.Join(", ",
                _contextGenerator.GenerateInstanceCreation(
                    overrideInstanceCreation: $"new {overrideContextName}({overrideParameters})",
                    outwardFacingTypeNumber: outwardFacingTypeIdReference,
                    caseNumber: initialCaseIdReference,
                    key: keyReference,
                    containerNode: containerNodeReference,
                    transientScopeNode: transientScopeNodeReference,
                    scopeNode: scopeNodeReference,
                    scopeNodeName: scopeNodeNameReference),
                Constants.TrueKeyword,
                Constants.TrueKeyword);

            code.AppendLine($"{(reference is null ? $"{concreteNode.Data.Type.FullName()} " : "")} {actualReference} = ({parameterDeclaration}) => {_sharedNameRegistry.GetEntryFunctionName(concreteNode.ReturnedElement.Target.Type)}({parameters});");
        }

        return actualReference;
    }
}
