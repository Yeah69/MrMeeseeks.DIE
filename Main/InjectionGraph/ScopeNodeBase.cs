using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal enum ScopeNodeType
{
    None,
    Container,
    TransientScope,
    Scope
}

internal class ScopedInstanceInterfaceDescription(ReferenceGenerator referenceGenerator) : IContainerInstance
{
    internal const string FunctionName = "Get";
    internal string InterfaceName { get; } = referenceGenerator.Generate("ScopedInstance");
}

internal record ScopedInstanceDescription(TypeNode TypeNode, IFunction SyncFunction, IFunction AsyncFunction);

internal record ScopeRootDescription(TypeNode TypeNode, IFunction SyncFunction, IFunction AsyncFunction);

internal abstract class ScopeNodeBase
{
    private readonly ScopedInstanceInterfaceDescription _scopedInstanceInterfaceDescription;
    private readonly Dictionary<TypeNode, ScopedInstanceDescription> _scopedInstances = [];
    
    public required WellKnownTypes WellKnownTypes { protected get; init; }

    internal ScopeNodeBase(ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription)
    {
        _scopedInstanceInterfaceDescription = scopedInstanceInterfaceDescription;
    }

    internal IReadOnlyCollection<ScopedInstanceDescription> ScopedInstances => _scopedInstances.Values;

    internal void AddScopedInstance(TypeNode node)
    {
        if (_scopedInstances.ContainsKey(node))
            return;
        var taskWrappedType = WellKnownTypes.ValueTask1 is not null
            ? WellKnownTypes.ValueTask1.Construct(node.Type)
            : WellKnownTypes.Task1.Construct(node.Type);
        _scopedInstances[node] = new(
            node,
            new ScopedInstanceFunction(node)
            {
                ExplicitInterface = new ExplicitInterfaceDescription.Generated($"{_scopedInstanceInterfaceDescription.InterfaceName}<{node.Type.FullName()}>")
            },
            new AsyncScopedInstanceFunction(node, taskWrappedType)
            {
                ExplicitInterface = new ExplicitInterfaceDescription.Generated($"{_scopedInstanceInterfaceDescription.InterfaceName}<{node.Type.FullName()}>")
            });
    }
}

internal sealed class ContainerScopeNode(ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription)
    : ScopeNodeBase(scopedInstanceInterfaceDescription);

internal abstract class NonContainerScopeNode(
    INamedTypeSymbol? type,
    string name,
    ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription) 
    : ScopeNodeBase(scopedInstanceInterfaceDescription)
{
    private readonly Dictionary<TypeNode, ScopeRootDescription> _scopeRoots = [];
    internal string Name { get; } = name;
    internal INamedTypeSymbol? Type { get; } = type;
    internal IEnumerable<ScopeRootDescription> ScopedRoots => _scopeRoots.Values;
    internal void AddScopeRoot(TypeNode node)
    {
        if (!_scopeRoots.ContainsKey(node))
        {
            var taskWrappedType = WellKnownTypes.ValueTask1 is not null
                ? WellKnownTypes.ValueTask1.Construct(node.Type)
                : WellKnownTypes.Task1.Construct(node.Type);
            _scopeRoots[node] = new(node, new ScopeRootFunction(node), new AsyncScopeRootFunction(node, taskWrappedType));
        }
    }
}

internal sealed class ScopeNode(
    INamedTypeSymbol? type,
    string name,
    ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription)
    : NonContainerScopeNode(type, name, scopedInstanceInterfaceDescription);

internal sealed class TransientScopeNode(
    INamedTypeSymbol? type,
    string name,
    ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription) 
    : NonContainerScopeNode(type, name, scopedInstanceInterfaceDescription);