using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.Utility;

internal static class FunctionResolutionUtility
{
    internal static T GetOrCreateFunction<T>(
        IDictionary<string, T> functionMap, 
        ITypeSymbol type, 
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters,
        Func<T> functionFactory) 
        where T : IFunctionResolutionBuilder
    {
        var key = $"{type.FullName()}:::{string.Join(":::", currentParameters.Select(p => p.Value.Item2.TypeFullName))}";
        if (!functionMap.TryGetValue(
                key,
                out var function))
        {
            if (functionMap
                    .Values
                    .Where(f =>
                        SymbolEqualityComparer.Default.Equals(f.OriginalReturnType, type)
                        && f.CurrentParameters.All(cp => currentParameters.Any(p =>
                            SymbolEqualityComparer.IncludeNullability.Equals(cp.Item1, p.Value.Item1))))
                    .OrderByDescending(f => f.CurrentParameters.Count)
                    .FirstOrDefault() is { } greatestCommonParameterSetFunction)
            {
                function = greatestCommonParameterSetFunction;
            }
            else
            {
                function = functionFactory();
                functionMap[key] = function;
            }
        }

        return function;
    }

    internal static TFunctionCallNode GetOrCreateFunctionCall<TFunctionNode, TFunctionCallNode>(
        ITypeSymbol type, 
        IFunctionNode callingFunction,
        IDictionary<TypeKey, List<TFunctionNode>> cache,
        Func<TFunctionNode> functionNodeFactory,
        Func<TFunctionNode, TFunctionCallNode> functionCallNodeFactory)
        where TFunctionNode : IFunctionNode
        where TFunctionCallNode : IFunctionCallNode
    {
        var typeKey = type.ToTypeKey();

        if (!cache.TryGetValue(typeKey, out var list))
        {
            list = new List<TFunctionNode>();
            cache[typeKey] = list;
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
        if (cache.Where(f => f.Parameters.All(k => callingFunction.Overrides.ContainsKey(k.Item2)))
                .OrderByDescending(f => f.Parameters)
                .FirstOrDefault()
            is {} match)
            return match;
        
        var function = functionNodeFactory();
        cache.Add(function);
        
        return function;
    }
}