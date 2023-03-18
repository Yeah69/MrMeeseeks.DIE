using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IFunctionCallNode : IElementNode, IAwaitableNode
{
    string? OwnerReference { get; }
    string FunctionName { get; }
    IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
}

internal abstract class FunctionCallNode : IFunctionCallNode
{
    private readonly IFunctionNode _calledFunction;

    public FunctionCallNode(
        string? ownerReference,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        
        IReferenceGenerator referenceGenerator)
    {
        _calledFunction = calledFunction;
        OwnerReference = ownerReference;
        Parameters = parameters;
        FunctionName = calledFunction.Name;
        Reference = referenceGenerator.Generate("functionCallResult");
    }

    public virtual void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName => 
        _calledFunction.AsyncTypeFullName 
        ?? _calledFunction.ReturnedTypeFullName;
    public string Reference { get; }
    public string FunctionName { get; }
    public virtual string? OwnerReference { get; }
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }

    public bool Awaited => _calledFunction.SynchronicityDecision is not SynchronicityDecision.Sync;
    public SynchronicityDecision SynchronicityDecision => _calledFunction.SynchronicityDecision;
}