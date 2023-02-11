using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IFunctionCallNode : IElementNode, IPotentiallyAwaitedNode
{
    string? OwnerReference { get; }
    string FunctionName { get; }
    IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    void MakeAsync(IOnAwait callingFunction);
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

    public string TypeFullName => _calledFunction.ReturnedTypeFullName;
    public string Reference { get; }
    public string FunctionName { get; }
    public virtual string? OwnerReference { get; }
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    public void MakeAsync(IOnAwait callingFunction)
    {
        Awaited = true;
        AsyncReference = Reference;
        AsyncTypeFullName = _calledFunction.AsyncTypeFullName;
        SynchronicityDecision = _calledFunction.SynchronicityDecision;
        callingFunction.OnAwait(this);
    }

    public bool Awaited { get; set; }
    public string? AsyncReference { get; private set; }
    public string? AsyncTypeFullName { get; private set; }
    public SynchronicityDecision SynchronicityDecision { get; private set; } = SynchronicityDecision.Sync;
}