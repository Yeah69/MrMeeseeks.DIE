using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryPropertyNode : IFactoryNodeBase;

internal sealed partial class FactoryPropertyNode : FactoryNodeBase, IFactoryPropertyNode
{
    internal FactoryPropertyNode(
        IPropertySymbol propertySymbol, 
        
        IFunctionNode parentFunction,
        ITaskBasedQueue taskBasedQueue,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes) 
        : base(propertySymbol.Type, propertySymbol, parentFunction, taskBasedQueue, referenceGenerator, wellKnownTypes)
    {
    }
}