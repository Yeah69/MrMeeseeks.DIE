using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface ILazyNode : IDelegateBaseNode
{
}

internal class LazyNode : DelegateBaseNode, ILazyNode
{
    internal LazyNode(
        INamedTypeSymbol lazyType,
        ILocalFunctionNode function,
        
        IDiagLogger diagLogger,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator) 
        : base(lazyType, function, diagLogger, parentContainer, referenceGenerator)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitLazyNode(this);
}