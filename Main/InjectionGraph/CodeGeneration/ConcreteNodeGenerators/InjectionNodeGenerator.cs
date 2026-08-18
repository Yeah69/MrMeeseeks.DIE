using System.Threading.Tasks;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class InjectionNodeGenerator(
    ConcreteNodeCodeGeneratorDispatcher concreteNodeCodeGeneratorDispatcher,
    ReferenceGenerator referenceGenerator,
    FunctionUtility functionUtility,
    WellKnownTypes wellKnownTypes) 
    : IContainerInstance
{
    public string GenerateForInjectionNode(StringBuilder code, TypeNode node, bool sync)
    {
        if (sync)
        {
            if (node.Outgoing is [{ TargetAsNode: ConcreteTaskNode { InnerEdge.TargetAsNode: TypeNode { AsyncFunction: { AsyncReturnType: {} asyncReturnType } asyncFunction } } }])
            {
                var innerIsValueTask = CustomSymbolEqualityComparer.Default.Equals(asyncReturnType.OriginalDefinition, wellKnownTypes.ValueTask1);
                var outerIsValueTask = CustomSymbolEqualityComparer.Default.Equals(node.Type.OriginalDefinition, wellKnownTypes.ValueTask1);

                var innerReference = CallFunction(code, asyncFunction, prefix: asyncReturnType.FullName(), await: false);
                
                if (innerIsValueTask == outerIsValueTask)
                    return innerReference;
                
                var outerReference = referenceGenerator.Generate(node.Type);
                var outerFullName = node.Type.FullName();

                var assignedValue = outerIsValueTask && !innerIsValueTask
                    ? $"new {outerFullName}({innerReference})"
                    : $"{innerReference}.{nameof(ValueTask<>.AsTask)}()";
                
                code.AppendLine($"{outerFullName} {outerReference} = {assignedValue};");
                return outerReference;
            }
            
            if (node is { Outgoing: [{ TargetAsNode: ConcreteAsyncEnumerableNode }], AsyncFunction: { AsyncReturnType: {} asyncReturnType0 } asyncFunction0})
                return CallFunction(code, asyncFunction0, asyncReturnType0.FullName(), await: false);
        }
        return concreteNodeCodeGeneratorDispatcher.GenerateForInjectionNode(code, node, sync);
    }

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
