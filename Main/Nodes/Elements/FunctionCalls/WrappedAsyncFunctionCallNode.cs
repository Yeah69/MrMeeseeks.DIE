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

    public WrappedAsyncFunctionCallNode(
        ITypeSymbol wrappedType,
        string? ownerReference,
        SynchronicityDecision synchronicityDecision,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        OwnerReference = ownerReference;
        FunctionName = calledFunction.Name;
        Parameters = parameters;
        TypeParameters = typeParameters;
        SynchronicityDecision = synchronicityDecision;
        _calledFunction = calledFunction;
        Transformation = synchronicityDecision is SynchronicityDecision.AsyncValueTask
            ? AsyncFunctionCallTransformation.ValueTaskFromValueTask
            : AsyncFunctionCallTransformation.TaskFromTask;

        var asyncType = synchronicityDecision is SynchronicityDecision.AsyncValueTask
            ? wellKnownTypes.ValueTask1 is not null 
                ? wellKnownTypes.ValueTask1.Construct(wrappedType)
                : throw new InvalidOperationException("ValueTask1 is not available")
            : wellKnownTypes.Task1.Construct(wrappedType);
        Reference = referenceGenerator.Generate(asyncType);
        TypeFullName = asyncType.FullName();
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
    public IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    public IFunctionNode CalledFunction => _calledFunction;
    public bool Awaited => false; // never awaited, because it's a wrapped async function call
}