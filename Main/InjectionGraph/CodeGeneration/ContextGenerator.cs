using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal sealed class ContextGenerator : IContainerInstance
{
    private const string OverridesConstructorParameterName = "overrides";
    private const string OutwardFacingTypeNumberConstructorParameterName = "outwardFacingTypeNr";
    private const string CaseNumberConstructorParameterName = "caseNr";
    private const string KeyConstructorParameterName = "key";
    private const string ContainerNodeConstructorParameterName = "containerNode";
    private const string TransientScopeNodeConstructorParameterName = "transientScopeNode";
    private const string ScopeNodeConstructorParameterName = "scopeNode";
    private readonly string _contextClassName;
    private readonly string _contextClassFullName;
    private readonly WellKnownTypes _wellKnownTypes;

    internal ContextGenerator(
        ContainerInfo containerInfo,
        ReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _wellKnownTypes = wellKnownTypes;
        _contextClassName = referenceGenerator.Generate("Context");
        _contextClassFullName = $"{containerInfo.FullName}.{_contextClassName}";
        ParameterName = referenceGenerator.Generate("context");
        FullNameAndParameterName = $"{_contextClassFullName} {ParameterName}";
        OverridesPropertyName = "Overrides";
        OutwardFacingTypeNumberPropertyName = "OutwardFacingTypeNumber";
        CaseNumberPropertyName = "CaseNumber";
        KeyPropertyName = "Key";
        ContainerNodePropertyName = "ContainerNode";
        TransientScopeNodePropertyName = "TransientScopeNode";
        ScopeNodePropertyName = "ScopeNode";
    }

    internal void GenerateContextClass(StringBuilder code) =>
        code.AppendLine(
            $$"""
              private class {{_contextClassName}}
              {
                internal {{_contextClassName}}({{_wellKnownTypes.Object.FullName()}} {{OverridesConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{OutwardFacingTypeNumberConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{CaseNumberConstructorParameterName}}, {{_wellKnownTypes.Object.WithNullableAnnotation(NullableAnnotation.Annotated).FullName()}} {{KeyConstructorParameterName}}, {{_wellKnownTypes.Object.FullName()}} {{ContainerNodeConstructorParameterName}}, {{_wellKnownTypes.Object.FullName()}} {{TransientScopeNodeConstructorParameterName}}, {{_wellKnownTypes.Object.FullName()}} {{ScopeNodeConstructorParameterName}})
                {
                  {{OverridesPropertyName}} = {{OverridesConstructorParameterName}};
                  {{OutwardFacingTypeNumberPropertyName}} = {{OutwardFacingTypeNumberConstructorParameterName}};
                  {{CaseNumberPropertyName}} = {{CaseNumberConstructorParameterName}};
                  {{KeyPropertyName}} = {{KeyConstructorParameterName}};
                  {{ContainerNodePropertyName}} = {{ContainerNodeConstructorParameterName}};
                  {{TransientScopeNodePropertyName}} = {{TransientScopeNodeConstructorParameterName}};
                  {{ScopeNodePropertyName}} = {{ScopeNodeConstructorParameterName}};
                }
                internal {{_wellKnownTypes.Object.FullName()}} {{OverridesPropertyName}} { get; }
                internal {{_wellKnownTypes.Int32.FullName()}} {{OutwardFacingTypeNumberPropertyName}} { get; }
                internal {{_wellKnownTypes.Int32.FullName()}} {{CaseNumberPropertyName}} { get; }
                internal {{_wellKnownTypes.Object.WithNullableAnnotation(NullableAnnotation.Annotated).FullName()}} {{KeyPropertyName}} { get; }
                internal {{_wellKnownTypes.Object.FullName()}} {{ContainerNodePropertyName}} { get; }
                internal {{_wellKnownTypes.Object.FullName()}} {{TransientScopeNodePropertyName}} { get; }
                internal {{_wellKnownTypes.Object.FullName()}} {{ScopeNodePropertyName}} { get; }
              }

              """);

    internal string GenerateInstanceCreation(string overrideInstanceCreation, string outwardFacingTypeNumber,
        string caseNumber, string key, string containerNode, string transientScopeNode, string scopeNode) =>
        $"new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreation}, {OutwardFacingTypeNumberConstructorParameterName}: {outwardFacingTypeNumber}, {CaseNumberConstructorParameterName}: {caseNumber}, {KeyConstructorParameterName}: {key}, {ContainerNodeConstructorParameterName}: {containerNode}, {TransientScopeNodeConstructorParameterName}: {transientScopeNode}, {ScopeNodeConstructorParameterName}: {scopeNode})";

    internal string GenerateCopyCreation(
        string? overrideInstanceCreation = null,
        string? outwardFacingTypeNumber = null,
        string? caseNumber = null,
        string? key = null,
        string? containerNode = null,
        string? transientScopeNode = null,
        string? scopeNode = null)
    {
        var overrideInstanceCreationString = overrideInstanceCreation ?? $"{ParameterName}.{OverridesPropertyName}";
        var outwardFacingTypeNumberString =
            outwardFacingTypeNumber ?? $"{ParameterName}.{OutwardFacingTypeNumberPropertyName}";
        var caseNumberString = caseNumber ?? $"{ParameterName}.{CaseNumberPropertyName}";
        var keyString = key ?? $"{ParameterName}.{KeyPropertyName}";
        var containerNodeString = containerNode ?? $"{ParameterName}.{ContainerNodePropertyName}";
        var transientScopeNodeString = transientScopeNode ?? $"{ParameterName}.{TransientScopeNodePropertyName}";
        var scopeNodeString = scopeNode ?? $"{ParameterName}.{ScopeNodePropertyName}";
        return GenerateInstanceCreation(overrideInstanceCreationString, outwardFacingTypeNumberString, caseNumberString, keyString, containerNodeString, transientScopeNodeString, scopeNodeString);
    }
    internal string GenerateCopyAssignment(
        string? overrideInstanceCreation = null,
        string? outwardFacingTypeNumber = null,
        string? caseNumber = null,
        string? key = null,
        string? containerNode = null,
        string? transientScopeNode = null,
        string? scopeNode = null) =>
        $"{ParameterName} = {GenerateCopyCreation(overrideInstanceCreation, outwardFacingTypeNumber, caseNumber, key, containerNode, transientScopeNode, scopeNode)};";

    internal string ParameterName { get; }
    internal string FullNameAndParameterName { get; }
    internal string OverridesPropertyName { get; }
    internal string OutwardFacingTypeNumberPropertyName { get; }
    internal string CaseNumberPropertyName { get; }
    internal string KeyPropertyName { get; }
    internal string ContainerNodePropertyName { get; }
    internal string TransientScopeNodePropertyName { get; }
    internal string ScopeNodePropertyName { get; }
}