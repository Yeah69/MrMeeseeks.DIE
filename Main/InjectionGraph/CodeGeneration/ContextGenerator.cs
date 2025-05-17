using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration;

internal class ContextGenerator
{
    private const string OverridesConstructorParameterName = "overrides";
    private const string InterfaceNumberConstructorParameterName = "interfaceNr";
    private const string ImplementationNumberConstructorParameterName = "implementationNr";
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
        InterfaceNumberPropertyName = "InterfaceNumber";
        ImplementationNumberPropertyName = "ImplementationNumber";
    }
    
    internal string GenerateContextClass() =>
        $$"""
          internal class {{_contextClassName}}
          {
            internal {{_contextClassName}}({{_wellKnownTypes.Object.FullName()}} {{OverridesConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{InterfaceNumberConstructorParameterName}}, {{_wellKnownTypes.Int32.FullName()}} {{ImplementationNumberConstructorParameterName}}) 
            {
              {{OverridesPropertyName}} = {{OverridesConstructorParameterName}};
              {{InterfaceNumberPropertyName}} = {{InterfaceNumberConstructorParameterName}};
              {{ImplementationNumberPropertyName}} = {{ImplementationNumberConstructorParameterName}};
            }
            internal {{_wellKnownTypes.Object.FullName()}} {{OverridesPropertyName}} { get; }
            internal {{_wellKnownTypes.Int32.FullName()}} {{InterfaceNumberPropertyName}} { get; }
            internal {{_wellKnownTypes.Int32.FullName()}} {{ImplementationNumberPropertyName}} { get; }
          }
          """;

    internal string GenerateInstanceCreation(string overrideInstanceCreation, int interfaceNumber, int implementationNumber) =>
        $"new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreation}, {InterfaceNumberConstructorParameterName}: {interfaceNumber}, {ImplementationNumberConstructorParameterName}: {implementationNumber})";

    internal string GenerateInstanceCopyAndAdjustment(string? overrideInstanceCreation = null, int? interfaceNumber = null, int? implementationNumber = null)
    {
        var overrideInstanceCreationString = overrideInstanceCreation ?? $"{ParameterName}.{OverridesPropertyName}";
        var interfaceNumberString = interfaceNumber.HasValue ? interfaceNumber.ToString() : $"{ParameterName}.{InterfaceNumberPropertyName}";
        var implementationNumberString = implementationNumber.HasValue ? implementationNumber.ToString() : $"{ParameterName}.{ImplementationNumberPropertyName}";
        return $"{ParameterName} = new {_contextClassFullName}({OverridesConstructorParameterName}: {overrideInstanceCreationString}, {InterfaceNumberConstructorParameterName}: {interfaceNumberString}, {ImplementationNumberConstructorParameterName}: {implementationNumberString});";
    }

    internal string ParameterName { get; }
    internal string FullNameAndParameterName { get; }
    internal string OverridesPropertyName { get; }
    internal string InterfaceNumberPropertyName { get; }
    internal string ImplementationNumberPropertyName { get; }
}