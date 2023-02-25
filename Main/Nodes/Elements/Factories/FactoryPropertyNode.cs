using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryPropertyNode : IFactoryNodeBase
{
}

internal class FactoryPropertyNode : FactoryNodeBase, IFactoryPropertyNode
{
    internal FactoryPropertyNode(
        IPropertySymbol propertySymbol, 
        IFunctionNode parentFunction,
        
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes) 
        : base(propertySymbol.Type, propertySymbol, parentFunction, referenceGenerator, wellKnownTypes)
    {
    }
    
    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitFactoryPropertyNode(this);
}