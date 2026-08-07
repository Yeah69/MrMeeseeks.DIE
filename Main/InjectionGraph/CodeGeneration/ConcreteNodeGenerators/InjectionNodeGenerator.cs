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
    public string GenerateForInjectionNode(StringBuilder code, TypeNode node, bool sync) =>
        _concreteNodeCodeGeneratorDispatcher.GenerateForInjectionNode(code, node, sync);

    public string CallFunctionOrGenerateForInjectionNode(StringBuilder code, IEdge edge, TypeNode node, bool sync)
    {
        var maybeFunction = sync 
            ? node.SyncFunction 
            : node.AsyncFunction;
        
        if (maybeFunction is not { } function) 
            return GenerateForInjectionNode(code, node, sync: sync);
        
        var resultReference = _referenceGenerator.Generate(function.RootNode.Type);
        var maybeAwait = !function.Sync && edge.SourceAsNode is not ConcreteTaskNode
            ? "await "
            : "";
        var prefix = function is { AsyncReturnType: {} asyncReturnType} && edge.SourceAsNode is ConcreteTaskNode 
            ? asyncReturnType.FullName()
            : function.RootNode.Type.FullName();
        code.AppendLine($"{prefix} {resultReference} = {maybeAwait}{_functionUtility.GenerateFunctionCall(function, doScopedInstance: true, doScopeRoot: true)};");
        return resultReference;
    }
}
