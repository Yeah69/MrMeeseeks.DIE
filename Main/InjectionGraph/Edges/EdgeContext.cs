using System.Diagnostics.CodeAnalysis;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal abstract record ScopeNodeContext
{
    internal sealed record Container : ScopeNodeContext;
    internal sealed record TransientScope(string TransientScopeName) : ScopeNodeContext;
    internal sealed record Scope(string ScopeName, string? TransientScopeName) : ScopeNodeContext;
}

internal abstract record OverrideContext
{
    internal sealed record None0 : OverrideContext;
    internal sealed record Any(ImmutableArray<ITypeSymbol> Overrides) : OverrideContext;
}

internal sealed class OverrideContextManager : IContainerInstance
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
            : new OverrideContext.None0();
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
    internal sealed record None1 : KeyContext;
    internal sealed record Single(ITypeSymbol Type, object Value) : KeyContext;
}

internal abstract record CaseChoiceContext
{
    internal sealed record None2 : CaseChoiceContext;
    internal sealed record Single(int OutwardFacingTypeId, int CaseId) : CaseChoiceContext;
}

internal sealed record EdgeContext(
    ScopeNodeContext ScopeNode,
    OverrideContext Override,
    KeyContext Key,
    CaseChoiceContext CaseChoice);