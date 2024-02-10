using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IPlainFunctionCallNode : IFunctionCallNode
{
}

internal partial class PlainFunctionCallNode : FunctionCallNode, IPlainFunctionCallNode
{
    public PlainFunctionCallNode(
        string? ownerReference,
        IFunctionNode calledFunction,
        ITypeSymbol callSideType,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        IReferenceGenerator referenceGenerator)
        : base(ownerReference, calledFunction, callSideType, parameters, typeParameters, referenceGenerator)
    {
    }
}