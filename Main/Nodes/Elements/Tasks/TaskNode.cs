using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Tasks;

internal interface ITaskNode : ITaskNodeBase
{
    
}

internal class TaskNode : TaskNodeBase, ITaskNode
{
    internal TaskNode(
        INamedTypeSymbol taskType,
        IContainerNode parentContainer,
        IFunctionNode parentFunction,
        IElementNodeMapperBase elementNodeMapperBase,
        IReferenceGenerator referenceGenerator)
        : base(taskType, parentContainer, parentFunction, elementNodeMapperBase, referenceGenerator)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitTaskNode(this);
}