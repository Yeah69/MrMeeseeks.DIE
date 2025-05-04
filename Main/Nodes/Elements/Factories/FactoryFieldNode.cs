using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryFieldNode : IFactoryNodeBase;

internal sealed partial class FactoryFieldNode : FactoryNodeBase,  IFactoryFieldNode
{
    internal FactoryFieldNode(
        IFieldSymbol fieldSymbol, 
        
        IFunctionNode parentFunction,
        ITaskBasedQueue taskBasedQueue,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes) 
        : base(fieldSymbol.Type, fieldSymbol, parentFunction, taskBasedQueue, referenceGenerator, wellKnownTypes)
    {
    }
}