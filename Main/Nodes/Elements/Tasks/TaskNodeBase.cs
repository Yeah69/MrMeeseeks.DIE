using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.Tasks;

internal enum AsyncWrappingStrategy
{
    VanillaFromResult,
    ImplementationFromValueTask,
    ImplementationFromTask,
    CallFromValueTask,
    CallFromTask,
    FactoryFromValueTask,
    FactoryFromTask,
    CollectionFromValueTask,
    CollectionFromTask
}

internal interface ITaskNodeBase : IElementNode
{
    IElementNode WrappedElement { get; }
    AsyncWrappingStrategy Strategy { get; }
    string? AsyncReference { get; }
    string ContainerTypeFullName { get; }
    ITaskTransformationFunctions TaskTransformationFunctions { get; }
    void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode);
}

internal abstract class TaskNodeBase : ITaskNodeBase
{
    private readonly INamedTypeSymbol _taskType;
    private readonly IFunctionNode _parentFunction;
    private readonly IElementNodeMapperBase _elementNodeMapperBase;
    internal TaskNodeBase(
        INamedTypeSymbol taskType,
        IContainerNode parentContainer,
        IFunctionNode parentFunction,
        IElementNodeMapperBase elementNodeMapperBase,
        
        IReferenceGenerator referenceGenerator)
    {
        _taskType = taskType;
        _parentFunction = parentFunction;
        _elementNodeMapperBase = elementNodeMapperBase;
        TypeFullName = taskType.FullName();
        Reference = referenceGenerator.Generate(_taskType);
        ContainerTypeFullName = parentContainer.FullName;
        TaskTransformationFunctions = parentContainer.TaskTransformationFunctions;
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        WrappedElement = _elementNodeMapperBase.Map(_taskType.TypeArguments.First(), implementationStack);
        if (WrappedElement is IPotentiallyAwaitedNode potentiallyAwaitingNode)
            _parentFunction.RegisterAsyncWrapping(potentiallyAwaitingNode, this);
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IElementNode WrappedElement { get; private set; } = null!;
    public AsyncWrappingStrategy Strategy { get; private set; } = AsyncWrappingStrategy.VanillaFromResult;
    public string? AsyncReference => (WrappedElement as IPotentiallyAwaitedNode)?.AsyncReference;
    public string ContainerTypeFullName { get; }
    public ITaskTransformationFunctions TaskTransformationFunctions { get; }

    public virtual void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode)
    {
        potentiallyAwaitedNode.Awaited = false;
        Strategy = potentiallyAwaitedNode switch
        {
            IImplementationNode { SynchronicityDecision: SynchronicityDecision.AsyncValueTask } => 
                AsyncWrappingStrategy.ImplementationFromValueTask,
            IImplementationNode { SynchronicityDecision: SynchronicityDecision.AsyncTask } => 
                AsyncWrappingStrategy.ImplementationFromTask,
            IFunctionCallNode { SynchronicityDecision: SynchronicityDecision.AsyncValueTask } => 
                AsyncWrappingStrategy.CallFromValueTask,
            IFunctionCallNode { SynchronicityDecision: SynchronicityDecision.AsyncTask } => 
                AsyncWrappingStrategy.CallFromTask,
            IFactoryNodeBase { SynchronicityDecision: SynchronicityDecision.AsyncValueTask } => 
                AsyncWrappingStrategy.FactoryFromValueTask,
            IFactoryNodeBase { SynchronicityDecision: SynchronicityDecision.AsyncTask } => 
                AsyncWrappingStrategy.FactoryFromTask,
            IEnumerableBasedNode { SynchronicityDecision: SynchronicityDecision.AsyncValueTask } =>
                AsyncWrappingStrategy.CollectionFromValueTask,
            IEnumerableBasedNode { SynchronicityDecision: SynchronicityDecision.AsyncTask } =>
                AsyncWrappingStrategy.CollectionFromTask,
            _ => AsyncWrappingStrategy.VanillaFromResult
        };

    }
}