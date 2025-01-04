using System.Diagnostics.CodeAnalysis;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal abstract record DomainContext
{
    internal record Container : DomainContext;
    internal record Scoped(string ScopeName) : DomainContext;
}

internal abstract record OverrideContext
{
    internal record None : OverrideContext;
    internal record Any(ImmutableArray<ITypeSymbol> Overrides) : OverrideContext;
}

internal class OverrideContextManager : IContainerInstance
{
    private readonly Dictionary<string, OverrideContext> _contexts = [];
    
    internal IReadOnlyCollection<OverrideContext> AllOverrideContexts => _contexts.Values;
    
    internal OverrideContext GetOrAddContext(IReadOnlyList<ITypeSymbol> overrides)
    {
        var consideredTypes = overrides
            .Distinct(CustomSymbolEqualityComparer.IncludeNullability)
            .OfType<ITypeSymbol>()
            .OrderBy(o => o.FullName())
            .ToImmutableArray();
        var key = string.Join(";", consideredTypes.Select(o => o.FullName()));
        if (_contexts.TryGetValue(key, out var context))
            return context;
        context = consideredTypes.Length > 0 
            ? new OverrideContext.Any(consideredTypes)
            : new OverrideContext.None();
        _contexts[key] = context;
        return context;
    }
    
    internal bool TryGetContext(IReadOnlyList<ITypeSymbol> overrides, [NotNullWhen(true)] out OverrideContext? context)
    {
        var consideredTypes = overrides
            .Distinct(CustomSymbolEqualityComparer.IncludeNullability)
            .OfType<ITypeSymbol>()
            .OrderBy(o => o.FullName())
            .ToImmutableArray();
        var key = string.Join(";", consideredTypes.Select(o => o.FullName()));
        return _contexts.TryGetValue(key, out context);
    }
}

internal abstract record KeyContext
{
    internal record None : KeyContext;
    internal record Single(ITypeSymbol KeyType, object KeyValue) : KeyContext;
}

internal record EdgeContext(DomainContext Domain, OverrideContext Override, KeyContext Key);