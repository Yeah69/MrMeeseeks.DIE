using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IPlainFunctionCallNode : IFunctionCallNode
{
}

internal partial class PlainFunctionCallNode(string? ownerReference,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReferenceGenerator referenceGenerator)
    : FunctionCallNode(ownerReference, calledFunction, parameters, referenceGenerator), IPlainFunctionCallNode;