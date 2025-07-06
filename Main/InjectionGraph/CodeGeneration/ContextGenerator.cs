using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal class ContextGenerator
{
    private const string OverridesConstructorParameterName = "overrides";
    private const string OutwardFacingTypeNumberConstructorParameterName = "outwardFacingTypeNr";
    private const string CaseNumberConstructorParameterName = "caseNr";
    private const string KeyConstructorParameterName = "key";
    private readonly string _contextClassName;
    private readonly string _contextClassFullName;
    private readonly WellKnownTypes _wellKnownTypes;

    internal ContextGenerator(
        IContainerInfo containerInfo,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _wellKnownTypes = wellKnownTypes;
        _contextClassName = referenceGenerator.Generate("Context");
        _contextClassFullName = $"global::{containerInfo.Namespace}.{containerInfo.Name}.{_contextClassName}";
        ParameterName = referenceGenerator.Generate("context");
        FullNameAndParameterName = $"{_contextClassFullName} {ParameterName}";
        OverridesPropertyName = "Overrides";
        OutwardFacingTypeNumberPropertyName = "OutwardFacingTypeNumber";
        CaseNumberPropertyName = "CaseNumber";
        KeyPropertyName = "Key";
    }

    internal string GenerateContextClass() =>
        $$"""
          internal class {{_contextClassName}}
          {
            internal {{_contextClassName}}({{_wellKnownTypes.Object.FullName()}} {{OverridesConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{OutwardFacingTypeNumberConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{CaseNumberConstructorParameterName}}, {{_wellKnownTypes.Object.WithNullableAnnotation(NullableAnnotation.Annotated).FullName()}} {{KeyConstructorParameterName}}) 
            {
              {{OverridesPropertyName}} = {{OverridesConstructorParameterName}};
              {{OutwardFacingTypeNumberPropertyName}} = {{OutwardFacingTypeNumberConstructorParameterName}};
              {{CaseNumberPropertyName}} = {{CaseNumberConstructorParameterName}};
              {{KeyPropertyName}} = {{KeyConstructorParameterName}};
            }
            internal {{_wellKnownTypes.Object.FullName()}} {{OverridesPropertyName}} { get; }
            internal {{_wellKnownTypes.Int32.FullName()}} {{OutwardFacingTypeNumberPropertyName}} { get; }
            internal {{_wellKnownTypes.Int32.FullName()}} {{CaseNumberPropertyName}} { get; }
            internal {{_wellKnownTypes.Object.WithNullableAnnotation(NullableAnnotation.Annotated).FullName()}} {{KeyPropertyName}} { get; }
          }
          """;

    internal string GenerateInstanceCreation(string overrideInstanceCreation, string outwardFacingTypeNumber,
        string caseNumber, string key) =>
        $"new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreation}, {OutwardFacingTypeNumberConstructorParameterName}: {outwardFacingTypeNumber}, {CaseNumberConstructorParameterName}: {caseNumber}, {KeyConstructorParameterName}: {key})";

    internal string GenerateInstanceCopyAndAdjustment(string? overrideInstanceCreation = null,
        string? outwardFacingTypeNumber = null, string? caseNumber = null, string? key = null)
    {
        var overrideInstanceCreationString = overrideInstanceCreation ?? $"{ParameterName}.{OverridesPropertyName}";
        var outwardFacingTypeNumberString =
            outwardFacingTypeNumber ?? $"{ParameterName}.{OutwardFacingTypeNumberPropertyName}";
        var caseNumberString = caseNumber ?? $"{ParameterName}.{CaseNumberPropertyName}";
        var keyString = key ?? $"{ParameterName}.{KeyPropertyName}";
        return
            $"{ParameterName} = new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreationString}, {OutwardFacingTypeNumberConstructorParameterName}: {outwardFacingTypeNumberString}, {CaseNumberConstructorParameterName}: {caseNumberString}, {KeyConstructorParameterName}: {keyString});";
    }

    internal string ParameterName { get; }
    internal string FullNameAndParameterName { get; }
    internal string OverridesPropertyName { get; }
    internal string OutwardFacingTypeNumberPropertyName { get; }
    internal string CaseNumberPropertyName { get; }
    internal string KeyPropertyName { get; }
}