using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IFunctionCallNode : IElementNode
{
    string? OwnerReference { get; }
    string FunctionName(ReturnTypeStatus returnTypeStatus);
    IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    (IElementNode Calling, IElementNode Called)? SubDisposalParameter { get; }
    (IElementNode Calling, IElementNode Called)? TransientScopeDisposalParameter { get; }
    IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    IFunctionNode CalledFunction { get; }
}

internal abstract class FunctionCallNode : IFunctionCallNode
{
    protected FunctionCallNode(
        string? ownerReference,
        ITypeSymbol callSideType,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReadOnlyList<ITypeSymbol> typeParameters,
        IElementNode? callingSubDisposal,
        IElementNode callingTransientScopeDisposal,
        
        IFunctionNode calledFunction,
        IReferenceGenerator referenceGenerator)
    {
        CalledFunction = calledFunction;
        OwnerReference = ownerReference;
        Parameters = parameters;
        TypeParameters = typeParameters;
        TypeFullName = callSideType.FullName();
        Reference = referenceGenerator.Generate("functionCallResult");
        SubDisposalParameter = callingSubDisposal is null || !calledFunction.IsSubDisposalAsParameter
            ? null 
            : (callingSubDisposal, calledFunction.SubDisposalNode);
        TransientScopeDisposalParameter = !calledFunction.IsTransientScopeDisposalAsParameter
            ? null
            : (callingTransientScopeDisposal, calledFunction.TransientScopeDisposalNode);
    }

    public virtual void Build(PassedContext passedContext) { }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }

    public string FunctionName(ReturnTypeStatus returnTypeStatus) => CalledFunction.Name(returnTypeStatus);
    public virtual string? OwnerReference { get; }
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    public (IElementNode Calling, IElementNode Called)? SubDisposalParameter { get; }
    public (IElementNode Calling, IElementNode Called)? TransientScopeDisposalParameter { get; }
    public IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    public IFunctionNode CalledFunction { get; }
}