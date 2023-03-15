using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
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

internal interface IAsyncFunctionCallNode : IFunctionCallNode
{
    AsyncFunctionCallTransformation Transformation { get; }
    void AdjustToCurrentCalledFunctionSynchronicity();
}

internal class AsyncFunctionCallNode : IAsyncFunctionCallNode
{
    private readonly IFunctionNode _calledFunction;

    public AsyncFunctionCallNode(
        ITypeSymbol wrappedType,
        string? ownerReference,
        SynchronicityDecision synchronicityDecision,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext)
    {
        OwnerReference = ownerReference;
        FunctionName = calledFunction.Name;
        Parameters = parameters;
        SynchronicityDecision = synchronicityDecision;
        _calledFunction = calledFunction;
        Transformation = synchronicityDecision is SynchronicityDecision.AsyncValueTask
            ? AsyncFunctionCallTransformation.ValueTaskFromValueTask
            : AsyncFunctionCallTransformation.TaskFromTask;

        var asyncType = synchronicityDecision is SynchronicityDecision.AsyncValueTask
            ? containerWideContext.WellKnownTypes.ValueTask1.Construct(wrappedType)
            : containerWideContext.WellKnownTypes.Task1.Construct(wrappedType);
        Reference = referenceGenerator.Generate(asyncType);
        TypeFullName = asyncType.FullName();
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitAsyncFunctionCallNode(this);
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
    public bool Awaited { get; } = false;
}