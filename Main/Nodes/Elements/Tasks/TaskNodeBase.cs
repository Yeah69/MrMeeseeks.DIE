using MrMeeseeks.DIE.Nodes.Elements.Factories;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
using MrMeeseeks.DIE.Visitors;

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
}

internal interface ITaskNodeBase : IElementNode
{
    IElementNode WrappedElement { get; }
    AsyncWrappingStrategy Strategy { get; }
    string? AsyncReference { get; }
    void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode);
}

internal abstract class TaskNodeBase : ITaskNodeBase
{
    private readonly INamedTypeSymbol _taskType;
    private readonly IFunctionNode _parentFunction;
    private readonly IElementNodeMapperBase _elementNodeMapperBase;
    internal TaskNodeBase(
        INamedTypeSymbol taskType,
        IFunctionNode parentFunction,
        IElementNodeMapperBase elementNodeMapperBase,
        IReferenceGenerator referenceGenerator)
    {
        _taskType = taskType;
        _parentFunction = parentFunction;
        _elementNodeMapperBase = elementNodeMapperBase;
        TypeFullName = taskType.FullName();
        Reference = referenceGenerator.Generate(_taskType);
    }

    public void Build()
    {
        WrappedElement = _elementNodeMapperBase.Map(_taskType.TypeArguments.First());
        if (WrappedElement is IPotentiallyAwaitedNode potentiallyAwaitingNode)
            _parentFunction.RegisterAsyncWrapping(potentiallyAwaitingNode, this);
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IElementNode WrappedElement { get; private set; } = null!;
    public AsyncWrappingStrategy Strategy { get; private set; } = AsyncWrappingStrategy.VanillaFromResult;
    public string? AsyncReference => (WrappedElement as IPotentiallyAwaitedNode)?.AsyncReference;
    
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
            _ => AsyncWrappingStrategy.VanillaFromResult
        };

    }
}