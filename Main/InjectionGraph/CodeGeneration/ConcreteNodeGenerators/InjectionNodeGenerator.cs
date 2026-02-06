using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class InjectionNodeGenerator : IContainerInstance
{
    private readonly ConcreteNodeCodeGeneratorDispatcher _concreteNodeCodeGeneratorDispatcher;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly FunctionUtility _functionUtility;

    internal InjectionNodeGenerator(
        ConcreteNodeCodeGeneratorDispatcher concreteNodeCodeGeneratorDispatcher,
        ReferenceGenerator referenceGenerator,
        FunctionUtility functionUtility)
    {
        _concreteNodeCodeGeneratorDispatcher = concreteNodeCodeGeneratorDispatcher;
        _referenceGenerator = referenceGenerator;
        _functionUtility = functionUtility;
    }

    public string GenerateForInjectionNode(StringBuilder code, TypeNode node) =>
        _concreteNodeCodeGeneratorDispatcher.GenerateForInjectionNode(code, node);

    public string CallFunctionOrGenerateForInjectionNode(StringBuilder code, TypeEdge edge, TypeNode node)
    {
        if (edge.Type is FunctionEdgeType functionEdgeType)
        {
            var function = functionEdgeType.Function;
            var resultReference = _referenceGenerator.Generate(function.RootNode.Type);
            code.AppendLine($"{function.RootNode.Type.FullName()} {resultReference} = {_functionUtility.GenerateFunctionCall(function, doScopedInstance: true, doScopeRoot: true)};");
            return resultReference;
        }
        return GenerateForInjectionNode(code, node);
    }
}
