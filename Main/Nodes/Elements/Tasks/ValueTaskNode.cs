using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Tasks;

internal interface IValueTaskNode : ITaskNodeBase
{
    
}

internal class ValueTaskNode : TaskNodeBase, IValueTaskNode
{
    internal ValueTaskNode(
        INamedTypeSymbol valueTaskType,
        IContainerNode parentContainer,
        IFunctionNode parentFunction,
        IElementNodeMapperBase elementNodeMapperBase,
        
        IReferenceGenerator referenceGenerator)
        : base(valueTaskType, parentContainer, parentFunction, elementNodeMapperBase, referenceGenerator)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitValueTaskNode(this);
}