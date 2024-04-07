using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal enum AsyncFunctionCallTransformation
{
    ValueTaskFromValueTask,
    ValueTaskFromTask,
    ValueTaskFromSync,
    TaskFromValueTask,
    TaskFromTask,
    TaskFromSync
}

internal interface IWrappedAsyncFunctionCallNode : IFunctionCallNode
{
    AsyncFunctionCallTransformation Transformation { get; }
    void AdjustToCurrentCalledFunctionSynchronicity();
}

internal sealed partial class WrappedAsyncFunctionCallNode : IWrappedAsyncFunctionCallNode
{
    private readonly IFunctionNode _calledFunction;

    internal record struct Params(
        ITypeSymbol WrappedType,
        string? OwnerReference,
        SynchronicityDecision SynchronicityDecision,
        IReadOnlyList<(IParameterNode, IParameterNode)> Parameters,
        IReadOnlyList<ITypeSymbol> TypeParameters,
        IElementNode CallingSubDisposal,
        IElementNode CallingTransientScopeDisposal);
    internal WrappedAsyncFunctionCallNode(
        Params parameters,
        
        IFunctionNode calledFunction,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        OwnerReference = parameters.OwnerReference;
        FunctionName = calledFunction.Name;
        Parameters = parameters.Parameters;
        TypeParameters = parameters.TypeParameters;
        SynchronicityDecision = parameters.SynchronicityDecision;
        _calledFunction = calledFunction;
        Transformation = parameters.SynchronicityDecision is SynchronicityDecision.AsyncValueTask
            ? AsyncFunctionCallTransformation.ValueTaskFromValueTask
            : AsyncFunctionCallTransformation.TaskFromTask;

        var asyncType = parameters.SynchronicityDecision is SynchronicityDecision.AsyncValueTask
            ? wellKnownTypes.ValueTask1 is not null 
                ? wellKnownTypes.ValueTask1.Construct(parameters.WrappedType)
                : throw new InvalidOperationException("ValueTask1 is not available")
            : wellKnownTypes.Task1.Construct(parameters.WrappedType);
        Reference = referenceGenerator.Generate(asyncType);
        TypeFullName = asyncType.FullName();
        SubDisposalParameter = !calledFunction.IsSubDisposalAsParameter
            ? null 
            : (parameters.CallingSubDisposal, calledFunction.SubDisposalNode);
        TransientScopeDisposalParameter = !calledFunction.IsTransientScopeDisposalAsParameter
            ? null
            : (parameters.CallingTransientScopeDisposal, calledFunction.TransientScopeDisposalNode);
    }

    public void Build(PassedContext passedContext) { }
    
    public AsyncFunctionCallTransformation Transformation { get; private set; }
    
    public void AdjustToCurrentCalledFunctionSynchronicity()
    {
        Transformation = (SynchronicityDecision, _calledFunction.SynchronicityDecision) switch
        {
            (SynchronicityDecision.AsyncValueTask, SynchronicityDecision.AsyncValueTask) => AsyncFunctionCallTransformation.ValueTaskFromValueTask,
            (SynchronicityDecision.AsyncValueTask, SynchronicityDecision.AsyncTask) => AsyncFunctionCallTransformation.ValueTaskFromTask,
            (SynchronicityDecision.AsyncValueTask, SynchronicityDecision.Sync) => AsyncFunctionCallTransformation.ValueTaskFromSync,
            (SynchronicityDecision.AsyncTask, SynchronicityDecision.AsyncValueTask) => AsyncFunctionCallTransformation.TaskFromValueTask,
            (SynchronicityDecision.AsyncTask, SynchronicityDecision.AsyncTask) => AsyncFunctionCallTransformation.TaskFromTask,
            (SynchronicityDecision.AsyncTask, SynchronicityDecision.Sync) => AsyncFunctionCallTransformation.TaskFromSync,
            _ => Transformation
        };
    }

    public string TypeFullName { get; }
    public string Reference { get; }
    public SynchronicityDecision SynchronicityDecision { get; }
    public string? OwnerReference { get; }
    public string FunctionName { get; }
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    public (IElementNode Calling, IElementNode Called)? SubDisposalParameter { get; }
    public (IElementNode Calling, IElementNode Called)? TransientScopeDisposalParameter { get; }
    public IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    public IFunctionNode CalledFunction => _calledFunction;
    public bool Awaited => false; // never awaited, because it's a wrapped async function call
}