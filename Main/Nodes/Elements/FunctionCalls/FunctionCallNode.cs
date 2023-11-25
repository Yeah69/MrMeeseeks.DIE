using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IFunctionCallNode : IElementNode, IAwaitableNode
{
    string? OwnerReference { get; }
    string FunctionName { get; }
    IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    IFunctionNode CalledFunction { get; }
}

internal abstract class FunctionCallNode(string? ownerReference,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReferenceGenerator referenceGenerator)
    : IFunctionCallNode
{
    public virtual void Build(PassedContext passedContext) { }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName => 
        calledFunction.AsyncTypeFullName 
        ?? calledFunction.ReturnedTypeFullName;
    public string Reference { get; } = referenceGenerator.Generate("functionCallResult");
    public string FunctionName { get; } = calledFunction.Name;
    public virtual string? OwnerReference { get; } = ownerReference;
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; } = parameters;
    public IFunctionNode CalledFunction => calledFunction;

    public bool Awaited => calledFunction.SynchronicityDecision is not SynchronicityDecision.Sync;
}