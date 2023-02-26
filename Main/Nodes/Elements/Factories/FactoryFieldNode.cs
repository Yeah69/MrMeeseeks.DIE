using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryFieldNode : IFactoryNodeBase
{
}

internal class FactoryFieldNode : FactoryNodeBase,  IFactoryFieldNode
{
    internal FactoryFieldNode(
        IFieldSymbol fieldSymbol, 
        IFunctionNode parentFunction,
        
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext) 
        : base(fieldSymbol.Type, fieldSymbol, parentFunction, referenceGenerator, containerWideContext)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitFactoryFieldNode(this);
}