using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class InjectionNodeGenerator(
    ConcreteNodeCodeGeneratorDispatcher concreteNodeCodeGeneratorDispatcher,
    ReferenceGenerator referenceGenerator,
    FunctionUtility functionUtility) 
    : IContainerInstance
{
    public string GenerateForInjectionNode(StringBuilder code, TypeNode node, bool sync) => 
        concreteNodeCodeGeneratorDispatcher.GenerateForInjectionNode(code, node, sync);

    public string CallFunctionOrGenerateForInjectionNode(StringBuilder code, TypeNode node, INode source, bool sync)
    {
        var maybeFunction = sync 
            ? node.SyncFunction 
            : node.AsyncFunction;

        if (maybeFunction is not { } function) 
            return GenerateForInjectionNode(code, node, sync: sync);
        
        var prefix = function is { AsyncReturnType: {} asyncReturnType} && source is ConcreteTaskNode 
            ? asyncReturnType.FullName()
            : function.RootNode.Type.FullName();
        
        return CallFunction(code, function, prefix: prefix, await: !sync);

    }

    private string CallFunction(StringBuilder code, ITypeNodeFunction function, string prefix, bool await)
    {
        var resultReference = referenceGenerator.Generate(function.RootNode.Type);
        var maybeAwait = await
            ? "await "
            : " ";
        code.AppendLine($"{prefix} {resultReference} = {maybeAwait}{functionUtility.GenerateFunctionCall(function, doScopedInstance: true, doScopeRoot: true)};");
        return resultReference;
    }
}
