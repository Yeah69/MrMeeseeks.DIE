using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface ILazyNode : IDelegateBaseNode
{
}

internal partial class LazyNode : DelegateBaseNode, ILazyNode
{
    internal LazyNode(
        INamedTypeSymbol lazyType,
        ILocalFunctionNode function,
        
        ILocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator) 
        : base(lazyType, function, localDiagLogger, parentContainer, referenceGenerator)
    {
    }
}