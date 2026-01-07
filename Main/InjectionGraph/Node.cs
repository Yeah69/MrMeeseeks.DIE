using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal enum NodeType
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

internal record ScopedInstanceDescription(TypeNode TypeNode, IFunction Function);

internal sealed class Node
{
    private readonly ScopedInstanceInterfaceDescription _scopedInstanceInterfaceDescription;
    private readonly List<ScopedInstanceDescription> _scopedInstances = [];

    internal Node(ScopedInstanceInterfaceDescription scopedInstanceInterfaceDescription)
    {
        _scopedInstanceInterfaceDescription = scopedInstanceInterfaceDescription;
        ScopedInstances = _scopedInstances.AsReadOnly();
    }
    
    internal IReadOnlyList<ScopedInstanceDescription> ScopedInstances { get; }

    internal void AddScopedInstance(TypeNode node) =>
        _scopedInstances.Add(new(node, new ScopedInstanceFunction(node)
        {
            ExplicitInterface = new ExplicitInterfaceDescription.Generated($"{_scopedInstanceInterfaceDescription.InterfaceName}<{node.Type.FullName()}>")
        }));
}