using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class FunctorNodeCodeGenerator(
    ContextGenerator contextGenerator,
    OverrideContextManager overrideContextManager,
    ReferenceGenerator referenceGenerator,
    SharedNameRegistry sharedNameRegistry,
    TypeParameterUtility typeParameterUtility,
    WellKnownTypes wellKnownTypes)
    : IConcreteNodeCodeGenerator<ConcreteFunctorNode>, IScopeInstance
{
    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteFunctorNode concreteNode, bool sync, string? reference = null)
    {
        var actualReference = reference ?? referenceGenerator.Generate(concreteNode.Data.Type);
        var parameterReferences = concreteNode.FunctorParameterTypes.Select(_ => referenceGenerator.Generate("p")).ToArray();
        var parameterDeclaration = string.Join(", ", parameterReferences);

        if (overrideContextManager.TryGetContext(concreteNode.FunctorParameterTypes, out var overrideContext)
            && sharedNameRegistry.TryGetOverrideContextName(overrideContext, out var overrideContextName))
        {
            var outwardFacingTypeIdReference = referenceGenerator.Generate("oId");
            var initialCaseIdReference = referenceGenerator.Generate("cId");
            var keyReference = referenceGenerator.Generate("kId");
            var containerNodeReference = referenceGenerator.Generate("cnId");
            var transientScopeNodeReference = referenceGenerator.Generate("tsnId");
            var scopeNodeReference = referenceGenerator.Generate("snId");
            var scopeNodeNameReference = referenceGenerator.Generate("snnId");

            code.AppendLine(
                $$"""
                  {{wellKnownTypes.Int32.FullName()}} {{outwardFacingTypeIdReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.OutwardFacingTypeNumberPropertyName}};
                  {{wellKnownTypes.Int32.FullName()}} {{initialCaseIdReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.CaseNumberPropertyName}};
                  {{wellKnownTypes.Object.WithNullableAnnotation(NullableAnnotation.Annotated).FullName()}} {{keyReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.KeyPropertyName}};
                  {{wellKnownTypes.Object.FullName()}} {{containerNodeReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.ContainerNodePropertyName}};
                  {{wellKnownTypes.Object.FullName()}} {{transientScopeNodeReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.TransientScopeNodePropertyName}};
                  {{wellKnownTypes.Object.FullName()}} {{scopeNodeReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.ScopeNodePropertyName}};
                  {{wellKnownTypes.String.FullName()}} {{scopeNodeNameReference}} = {{contextGenerator.ParameterName}}.{{contextGenerator.ScopeNodeNamePropertyName}};
                  """);

            var overrideParameters = overrideContext is OverrideContext.Any any
                ? string.Join(", ", any.Overrides.Select(o => parameterReferences[concreteNode.FunctorParameterTypes.Select((t, i) => (t, i)).First(t => CustomSymbolEqualityComparer.IncludeNullability.Equals(t.t, o)).i]))
                : "";

            var parameters = string.Join(", ",
                contextGenerator.GenerateInstanceCreation(
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

            var genericTypeParameters = typeParameterUtility.ExtractTypeParameters(concreteNode.ReturnedElement.Target.Type) switch
            {
                [] => "",
                [..] items => $"<{string.Join(", ", items.Select(i => i.Name))}>" 
            };
            
            code.AppendLine($"{(reference is null ? $"{concreteNode.Data.Type.FullName()} " : "")} {actualReference} = ({parameterDeclaration}) => {sharedNameRegistry.GetEntryFunctionName(concreteNode.ReturnedElement.Target.Type)}{genericTypeParameters}({parameters});");
        }

        return actualReference;
    }
}
