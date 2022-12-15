using MrMeeseeks.DIE.Nodes.Functions;
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
        IReferenceGenerator referenceGenerator) 
        : base(lazyType, function, referenceGenerator)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitLazyNode(this);
}