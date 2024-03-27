using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IPlainFunctionCallNode : IFunctionCallNode;

internal sealed partial class PlainFunctionCallNode : FunctionCallNode, IPlainFunctionCallNode
{
    internal record struct Params(
        ITypeSymbol CallSideType,
        string? OwnerReference,
        IReadOnlyList<(IParameterNode, IParameterNode)> Parameters,
        IReadOnlyList<ITypeSymbol> TypeParameters,
        IElementNode CallingSubDisposal,
        IElementNode CallingTransientScopeDisposal);
    internal PlainFunctionCallNode(
        Params parameters,
        
        IFunctionNode calledFunction,
        IReferenceGenerator referenceGenerator)
        : base(
            parameters.OwnerReference,
            parameters.CallSideType,
            parameters.Parameters,
            parameters.TypeParameters,
            parameters.CallingSubDisposal,
            parameters.CallingTransientScopeDisposal,
            calledFunction,
            referenceGenerator)
    {
    }
}