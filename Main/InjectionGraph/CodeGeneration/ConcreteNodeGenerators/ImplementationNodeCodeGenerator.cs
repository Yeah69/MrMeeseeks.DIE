using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class ImplementationNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteImplementationNode>, IContainerInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly ContextGenerator _contextGenerator;

    internal ImplementationNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ReferenceGenerator referenceGenerator,
        ContextGenerator contextGenerator)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _referenceGenerator = referenceGenerator;
        _contextGenerator = contextGenerator;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteImplementationNode concreteNode, string? reference = null)
    {
        var referenceIsExternal = reference is not null;
        var actualReference = reference ?? _referenceGenerator.Generate(concreteNode.Data.Implementation);
        var referenceOriginalContext = _referenceGenerator.Generate("originalContext");
        var referencePurgedContext = _referenceGenerator.Generate("purgedContext");

        if (concreteNode.NeedsOriginalContextReference)
        {
            code.AppendLine(
                $$"""
                  var {{referenceOriginalContext}} = {{_contextGenerator.ParameterName}};
                  {{_contextGenerator.ParameterName}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} != 0
                    ? {{_contextGenerator.GenerateCopyCreation(outwardFacingTypeNumber: "0", caseNumber: "0")}}
                    : {{_contextGenerator.ParameterName}};
                  var {{referencePurgedContext}} = {{_contextGenerator.ParameterName}};
                  """);
        }
        else if (concreteNode.NeedsPurge)
        {
            code.AppendLine(
                $$"""
                  {{_contextGenerator.ParameterName}} = {{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} != 0
                    ? {{_contextGenerator.GenerateCopyCreation(outwardFacingTypeNumber: "0", caseNumber: "0")}}
                    : {{_contextGenerator.ParameterName}};
                  """);
        }

        // Constructor
        var parameters = string.Join(", ", concreteNode.ConstructorParameters.Select(d => HandleImplementationDependency(code, d, referenceOriginalContext, referencePurgedContext)));

        // Object initializer
        var objectInitializer = "";
        if (concreteNode.ObjectInitializerAssignments.Length > 0)
        {
            var propertyNodeAssignments = concreteNode.ObjectInitializerAssignments;
            objectInitializer = $" {{ {string.Join(", ", propertyNodeAssignments.Select(d => $"{d.Name} = {HandleImplementationDependency(code, d, referenceOriginalContext, referencePurgedContext)}"))} }}";
        }

        var implementationFullName = GetImplementationsFullName(concreteNode.Data.Implementation);
        code.AppendLine($"{(referenceIsExternal ? "" : $"{implementationFullName} ")}{actualReference} = new {implementationFullName}({parameters}){objectInitializer};");
        
        if (concreteNode.Data.Initializer is {} initializer)
        {
            var prefix = ""; // Todo should be "await" for (Value)Task-Initializer
            var initializerParameters = string.Join(", ", concreteNode.InitializerParameters.Select(d => $"{d.Name}: {HandleImplementationDependency(code, d, referenceOriginalContext, referencePurgedContext)}"));
            code.AppendLine($"{prefix}(({initializer.Type.FullName()}) {actualReference}).{initializer.Method.Name}({initializerParameters});");
        }

        return actualReference;
    }

    private string HandleImplementationDependency(
        StringBuilder code,
        ConcreteImplementationNode.Dependency dependency,
        string referenceOriginalContext,
        string referencePurgedContext)
    {
        if (dependency.PassOriginalChoiceContextId is not null)
        {
            code.AppendLine(
                $$"""
                  {{_contextGenerator.ParameterName}} = {{referenceOriginalContext}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} == {{dependency.PassOriginalChoiceContextId}}
                    ? {{referenceOriginalContext}}
                    : {{_contextGenerator.ParameterName}};
                  """);
        }

        var ret = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, dependency.Edge, dependency.Edge.Target);

        if (dependency.PassOriginalChoiceContextId is not null)
        {
            code.AppendLine($"{_contextGenerator.ParameterName} = {referencePurgedContext};");
        }

        return ret;
    }

    private static string GetImplementationsFullName(ITypeSymbol implementation)
    {
        var implementationFullName = implementation.FullName();
        if (!implementationFullName.StartsWith("(", StringComparison.InvariantCulture)
            || !implementationFullName.EndsWith(")", StringComparison.InvariantCulture)
            || implementation is not INamedTypeSymbol namedType)
            return implementationFullName;
        var namespaceFullName = implementation.ContainingNamespace.FullName();
        var typeName = implementation.Name;
        var typeParameters = namedType.TypeArguments.Length > 0
            ? $"<{string.Join(", ", namedType.TypeArguments.Select(GetImplementationsFullName))}>"
            : "";
        return $"{namespaceFullName}.{typeName}{typeParameters}";
    }
}
