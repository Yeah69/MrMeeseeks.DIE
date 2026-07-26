using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class InjectionNodeGenerator : IScopeInstance
{
    private readonly ConcreteNodeCodeGeneratorDispatcher _concreteNodeCodeGeneratorDispatcher;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly FunctionUtility _functionUtility;
    private readonly GraphTypeHolder _graphTypeHolder;

    internal InjectionNodeGenerator(
        ConcreteNodeCodeGeneratorDispatcher concreteNodeCodeGeneratorDispatcher,
        ReferenceGenerator referenceGenerator,
        FunctionUtility functionUtility,
        GraphTypeHolder graphTypeHolder)
    {
        _concreteNodeCodeGeneratorDispatcher = concreteNodeCodeGeneratorDispatcher;
        _referenceGenerator = referenceGenerator;
        _functionUtility = functionUtility;
        _graphTypeHolder = graphTypeHolder;
    }

    public string GenerateForInjectionNode(StringBuilder code, TypeNode node) =>
        _concreteNodeCodeGeneratorDispatcher.GenerateForInjectionNode(code, node);

    public string CallFunctionOrGenerateForInjectionNode(StringBuilder code, TypeEdge edge, TypeNode node)
    {
        if (edge.Type is FunctionEdgeType functionEdgeType)
        {
            var function = functionEdgeType.Function;
            var resultReference = _referenceGenerator.Generate(function.RootNode.Type);
            var maybeAwait = function is AsyncTypeNodeFunction && edge.Source is not ConcreteTaskNode
                ? "await "
                : "";
            var prefix = function is AsyncTypeNodeFunction { AsyncReturnType: {} asyncReturnType} && edge.Source is ConcreteTaskNode 
                ? asyncReturnType.FullName()
                : function.RootNode.Type.FullName();
            code.AppendLine($"{prefix} {resultReference} = {maybeAwait}{_functionUtility.GenerateFunctionCall(function, doScopedInstance: true, doScopeRoot: true)};");
            return resultReference;
        }
        return GenerateForInjectionNode(code, node);
    }
}
