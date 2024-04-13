using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.CodeGeneration.Nodes;

internal interface ITransientScopeNodeGenerator : IScopeNodeBaseGenerator;

internal sealed class TransientScopeNodeGenerator : ScopeNodeBaseGenerator, ITransientScopeNodeGenerator
{
    private readonly ITransientScopeNode _transientScopeNode;
    private readonly IContainerNode _containerNode;

    internal TransientScopeNodeGenerator(
        ITransientScopeNode transientScopeNode,
        IContainerNode containerNode,
        IDisposeUtility disposeUtility,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections) 
        : base(
            transientScopeNode,
            containerNode,
            disposeUtility,
            wellKnownTypes,
            wellKnownTypesCollections)
    {
        _transientScopeNode = transientScopeNode;
        _containerNode = containerNode;
    }

    protected override string InterfaceAssignment => _containerNode.TransientScopeInterface.Name;

    protected override void PreGeneralContent(StringBuilder code, ICodeGenerationVisitor visitor)
    {
        code.AppendLine($"internal required {_transientScopeNode.ContainerFullName} {_transientScopeNode.ContainerReference} {{ private get; init; }}");
    }
}