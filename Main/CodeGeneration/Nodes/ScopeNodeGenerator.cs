using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface IScopeNodeGenerator : IScopeNodeBaseGenerator;

internal sealed class ScopeNodeGenerator : ScopeNodeBaseGenerator, IScopeNodeGenerator
{
    private readonly IScopeNode _scopeNode;
    private readonly IContainerNode _containerNode;

    internal ScopeNodeGenerator(
        IScopeNode scopeNode,
        IContainerNode containerNode,
        IDisposeUtility disposeUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
        : base(
            scopeNode,
            containerNode,
            disposeUtility,
            wellKnownTypes,
            wellKnownTypesCollections)
    {
        _scopeNode = scopeNode;
        _containerNode = containerNode;
    }

    protected override string InterfaceAssignment => _containerNode.ScopeInterface;

    protected override void PreGeneralContent(StringBuilder code, ICodeGenerationVisitor visitor)
    {
        code.AppendLine(
            $$"""
              internal required {{_scopeNode.ContainerFullName}} {{_scopeNode.ContainerReference}} { private get; init; }
              internal required {{_scopeNode.TransientScopeInterfaceName}} {{_scopeNode.TransientScopeInterfaceReference}} { private get; init; }
              """);
    }
}