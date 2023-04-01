using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IPlainFunctionCallNode : IFunctionCallNode
{
}

internal class PlainFunctionCallNode : FunctionCallNode, IPlainFunctionCallNode
{
    public PlainFunctionCallNode(
        string? ownerReference,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        
        IReferenceGenerator referenceGenerator)
        : base(ownerReference, calledFunction, parameters, referenceGenerator)
    {
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitIPlainFunctionCallNode(this);
}