using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class ImplementationNodeCodeGenerator(
    Lazy<InjectionNodeGenerator> injectionNodeGenerator,
    ReferenceGenerator referenceGenerator,
    ContextGenerator contextGenerator,
    WellKnownTypes wellKnownTypes) 
    : IConcreteNodeCodeGenerator<ConcreteImplementationNode>, IScopeInstance
{
    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteImplementationNode concreteNode, bool sync, string? reference = null)
    {
        var referenceIsExternal = reference is not null;
        var actualReference = reference ?? referenceGenerator.Generate(concreteNode.Data.Implementation);
        var referenceOriginalContext = referenceGenerator.Generate("originalContext");
        var referencePurgedContext = referenceGenerator.Generate("purgedContext");

        if (concreteNode.NeedsOriginalContextReference)
        {
            code.AppendLine(
                $$"""
                  var {{referenceOriginalContext}} = {{contextGenerator.ParameterName}};
                  {{contextGenerator.ParameterName}} = {{contextGenerator.ParameterName}}.{{contextGenerator.OutwardFacingTypeNumberPropertyName}} != 0
                    ? {{contextGenerator.GenerateCopyCreation(outwardFacingTypeNumber: "0", caseNumber: "0")}}
                    : {{contextGenerator.ParameterName}};
                  var {{referencePurgedContext}} = {{contextGenerator.ParameterName}};
                  """);
        }
        else if (concreteNode.NeedsPurge)
        {
            code.AppendLine(
                $$"""
                  {{contextGenerator.ParameterName}} = {{contextGenerator.ParameterName}}.{{contextGenerator.OutwardFacingTypeNumberPropertyName}} != 0
                    ? {{contextGenerator.GenerateCopyCreation(outwardFacingTypeNumber: "0", caseNumber: "0")}}
                    : {{contextGenerator.ParameterName}};
                  """);
        }

        // Constructor
        var parameters = string.Join(", ", concreteNode.ConstructorParameters.Select(d => HandleImplementationDependency(code, d, referenceOriginalContext, referencePurgedContext, sync: sync)));

        // Object initializer
        var objectInitializer = "";
        if (concreteNode.ObjectInitializerAssignments.Length > 0)
        {
            var propertyNodeAssignments = concreteNode.ObjectInitializerAssignments;
            objectInitializer = $" {{ {string.Join(", ", propertyNodeAssignments.Select(d => $"{d.Name} = {HandleImplementationDependency(code, d, referenceOriginalContext, referencePurgedContext, sync: sync)}"))} }}";
        }

        var implementationFullName = GetImplementationsFullName(concreteNode.Data.Implementation);
        code.AppendLine($"{(referenceIsExternal ? "" : $"{implementationFullName} ")}{actualReference} = new {implementationFullName}({parameters}){objectInitializer};");
        
        if (concreteNode.Data.Initializer is { Method.ReturnType: {} returnType} initializer)
        {
            var prefix = CustomSymbolEqualityComparer.Default.Equals(returnType, wellKnownTypes.Task)
                         || CustomSymbolEqualityComparer.Default.Equals(returnType, wellKnownTypes.ValueTask)
                ? "await "
                : "";
            var initializerParameters = string.Join(", ", concreteNode.InitializerParameters.Select(d => $"{d.Name}: {HandleImplementationDependency(code, d, referenceOriginalContext, referencePurgedContext, sync: sync)}"));
            code.AppendLine($"{prefix}(({initializer.Type.FullName()}) {actualReference}).{initializer.Method.Name}({initializerParameters});");
        }

        return actualReference;
    }

    private string HandleImplementationDependency(
        StringBuilder code,
        ConcreteImplementationNode.Dependency dependency,
        string referenceOriginalContext,
        string referencePurgedContext,
        bool sync)
    {
        if (dependency.PassOriginalChoiceContextId is not null)
        {
            code.AppendLine(
                $$"""
                  {{contextGenerator.ParameterName}} = {{referenceOriginalContext}}.{{contextGenerator.OutwardFacingTypeNumberPropertyName}} == {{dependency.PassOriginalChoiceContextId}}
                    ? {{referenceOriginalContext}}
                    : {{contextGenerator.ParameterName}};
                  """);
        }

        var ret = injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, dependency.Edge, dependency.Edge.Target, sync: sync);

        if (dependency.PassOriginalChoiceContextId is not null)
        {
            code.AppendLine($"{contextGenerator.ParameterName} = {referencePurgedContext};");
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
