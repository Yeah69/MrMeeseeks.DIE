using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IFuncNode : IDelegateBaseNode
{
    
}

internal class FuncNode : DelegateBaseNode, IFuncNode
{
    internal FuncNode(
        INamedTypeSymbol funcType,
        ILocalFunctionNode function,
        IReferenceGenerator referenceGenerator) 
        : base(funcType, function, referenceGenerator)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitFuncNode(this);
}