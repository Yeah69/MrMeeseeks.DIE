using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal class ContextGenerator
{
    private const string OverridesConstructorParameterName = "overrides";
    private const string OutwardFacingTypeNumberConstructorParameterName = "outwardFacingTypeNr";
    private const string CaseNumberConstructorParameterName = "caseNr";
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
    }
    
    internal string GenerateContextClass() =>
        $$"""
          internal class {{_contextClassName}}
          {
            internal {{_contextClassName}}({{_wellKnownTypes.Object.FullName()}} {{OverridesConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{OutwardFacingTypeNumberConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{CaseNumberConstructorParameterName}}) 
            {
              {{OverridesPropertyName}} = {{OverridesConstructorParameterName}};
              {{OutwardFacingTypeNumberPropertyName}} = {{OutwardFacingTypeNumberConstructorParameterName}};
              {{CaseNumberPropertyName}} = {{CaseNumberConstructorParameterName}};
            }
            internal {{_wellKnownTypes.Object.FullName()}} {{OverridesPropertyName}} { get; }
            internal {{_wellKnownTypes.Int32.FullName()}} {{OutwardFacingTypeNumberPropertyName}} { get; }
            internal {{_wellKnownTypes.Int32.FullName()}} {{CaseNumberPropertyName}} { get; }
          }
          """;

    internal string GenerateInstanceCreation(string overrideInstanceCreation, string outwardFacingTypeNumber, string caseNumber) =>
        $"new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreation}, {OutwardFacingTypeNumberConstructorParameterName}: {outwardFacingTypeNumber}, {CaseNumberConstructorParameterName}: {caseNumber})";

    internal string GenerateInstanceCopyAndAdjustment(string? overrideInstanceCreation = null, string? outwardFacingTypeNumber = null, string? caseNumber = null)
    {
        var overrideInstanceCreationString = overrideInstanceCreation ?? $"{ParameterName}.{OverridesPropertyName}";
        var outwardFacingTypeNumberString = outwardFacingTypeNumber ?? $"{ParameterName}.{OutwardFacingTypeNumberPropertyName}";
        var caseNumberString = caseNumber ?? $"{ParameterName}.{CaseNumberPropertyName}";
        return $"{ParameterName} = new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreationString}, {OutwardFacingTypeNumberConstructorParameterName}: {outwardFacingTypeNumberString}, {CaseNumberConstructorParameterName}: {caseNumberString});";
    }

    internal string ParameterName { get; }
    internal string FullNameAndParameterName { get; }
    internal string OverridesPropertyName { get; }
    internal string OutwardFacingTypeNumberPropertyName { get; }
    internal string CaseNumberPropertyName { get; }
}