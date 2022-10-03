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
}