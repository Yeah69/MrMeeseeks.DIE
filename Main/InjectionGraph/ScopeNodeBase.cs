using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal class ScopedInstanceInterfaceDescription(ReferenceGenerator referenceGenerator) : IContainerInstance
{
    internal const string SyncFunctionName = "Get";
    internal string SyncInterfaceName { get; } = referenceGenerator.Generate("SyncScopedInstance");
    internal const string AsyncFunctionName = "GetAsync";
    internal string AsyncInterfaceName { get; } = referenceGenerator.Generate("AsyncScopedInstance");
}

internal record ScopeRootDescription(TypeNode TypeNode, IFunction SyncFunction, IFunction AsyncFunction);

internal abstract class ScopeNodeBase;

internal sealed class ContainerScopeNode : ScopeNodeBase;

internal abstract class NonContainerScopeNode(
    INamedTypeSymbol? type,
    string name) 
    : ScopeNodeBase
{
    private readonly Dictionary<TypeNode, ScopeRootDescription> _scopeRoots = [];
    internal required Func<TypeNode, bool, ScopeRootFunction> ScopeRootFunctionFactory { private get; init; }
    internal string Name { get; } = name;
    internal INamedTypeSymbol? Type { get; } = type;
    internal IEnumerable<ScopeRootDescription> ScopedRoots => _scopeRoots.Values;
    internal void AddScopeRoot(TypeNode node)
    {
        if (!_scopeRoots.ContainsKey(node))
            _scopeRoots[node] = new(node, ScopeRootFunctionFactory(node, true/*Sync*/), ScopeRootFunctionFactory(node, false/*Async*/));
    }
}

internal sealed class ScopeNode(
    INamedTypeSymbol? type,
    string name)
    : NonContainerScopeNode(type, name);

internal sealed class TransientScopeNode(
    INamedTypeSymbol? type,
    string name) 
    : NonContainerScopeNode(type, name);