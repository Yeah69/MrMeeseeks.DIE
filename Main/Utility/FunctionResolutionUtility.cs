using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Utility;

internal static class FunctionResolutionUtility
{
    internal static TFunctionCallNode GetOrCreateFunctionCall<TFunctionNode, TFunctionCallNode>(
        ITypeSymbol key,
        IFunctionNode callingFunction,
        IDictionary<ITypeSymbol, List<TFunctionNode>> cache,
        Func<TFunctionNode> functionNodeFactory,
        Func<TFunctionNode, TFunctionCallNode> functionCallNodeFactory)
        where TFunctionNode : IFunctionNode
        where TFunctionCallNode : IFunctionCallNode
    {
        if (!cache.TryGetValue(key, out var list))
        {
            list = new List<TFunctionNode>();
            cache[key] = list;
        }

        var function = GetOrCreateFunction(
            callingFunction,
            list,
            functionNodeFactory);
        
        return functionCallNodeFactory(function);
    }
    
    internal static TFunctionNode GetOrCreateFunction<TFunctionNode>(
        IFunctionNode callingFunction,
        IList<TFunctionNode> cache,
        Func<TFunctionNode> functionNodeFactory)
        where TFunctionNode : IFunctionNode
    {
        if (cache.Where(f => f.Parameters.All(k => callingFunction.Overrides.ContainsKey(k.Type)))
                .OrderByDescending(f => f.Parameters.Count)
                .FirstOrDefault()
            is {} match)
            return match;
        
        var function = functionNodeFactory();
        cache.Add(function);
        
        return function;
    }
}