using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryPropertyNode : IFactoryNodeBase
{
}

internal partial class FactoryPropertyNode : FactoryNodeBase, IFactoryPropertyNode
{
    internal FactoryPropertyNode(
        IPropertySymbol propertySymbol, 
        
        IFunctionNode parentFunction,
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext) 
        : base(propertySymbol.Type, propertySymbol, parentFunction, referenceGenerator, containerWideContext)
    {
    }
}