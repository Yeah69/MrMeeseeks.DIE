using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IFuncNode : IDelegateBaseNode
{
    
}

internal partial class FuncNode : DelegateBaseNode, IFuncNode
{
    internal FuncNode(
        INamedTypeSymbol funcType,
        ILocalFunctionNode function,
        
        ILocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator) 
        : base(funcType, function, localDiagLogger, parentContainer, referenceGenerator)
    {
    }
}