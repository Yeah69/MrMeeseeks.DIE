using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IVoidFunctionNode : IFunctionNode
{
    IReadOnlyList<(IFunctionCallNode, IInitializedInstanceNode)> Initializations { get; }
    void ReorderOrDetectCycle();
}

internal sealed partial class VoidFunctionNode : FunctionNodeBase, IVoidFunctionNode, IScopeInstance
{
    private readonly IReadOnlyList<IInitializedInstanceNode> _initializedInstanceNodes;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly IRangeNode _parentRange;

    internal VoidFunctionNode(
        // parameters
        IReadOnlyList<IInitializedInstanceNode> initializedInstanceNodes,
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator,
        ILocalDiagLogger localDiagLogger,
        IOuterFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        WellKnownTypes wellKnownTypes)
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal, 
            parameters, 
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentContainer, 
            parentRange,
            subDisposalNodeChooser,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            wellKnownTypes)
    {
        _initializedInstanceNodes = initializedInstanceNodes;
        _localDiagLogger = localDiagLogger;
        _parentRange = parentRange;
        ReturnedTypeFullName = "void";
        Name = referenceGenerator.Generate("Initialize");
    }
    
    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        Initializations = _initializedInstanceNodes
            .Select(i => (i.BuildCall(_parentRange, this), i))
            .ToList();
    }

    protected override void AdjustToAsync()
    {
        if (WellKnownTypes.ValueTask is not null)
        {
            SynchronicityDecision = SynchronicityDecision.AsyncValueTask;
            ReturnedTypeFullName = WellKnownTypes.ValueTask.FullName();
        }
        else
        {
            SynchronicityDecision = SynchronicityDecision.AsyncTask;
            ReturnedTypeFullName = WellKnownTypes.Task.FullName();
        }
    }

    public override bool CheckIfReturnedType(ITypeSymbol type) => false;

    public override string Name { get; protected set; }
    public override string ReturnedTypeNameNotWrapped => "void";

    public IReadOnlyList<(IFunctionCallNode, IInitializedInstanceNode)> Initializations { get; private set; } =
        Array.Empty<(IFunctionCallNode, IInitializedInstanceNode)>();

    public void ReorderOrDetectCycle()
    {
        var map = new Dictionary<IInitializedInstanceNode, IReadOnlyList<IInitializedInstanceNode>>();
        foreach (var (initializationFunctionCall, initializedInstance) in Initializations)
        {
            var usedInitializedInstances = SelfAndCalledFunctions(initializationFunctionCall.CalledFunction)
                .SelectMany(f => f.UsedInitializedInstance)
                .ToList();
            map[initializedInstance] = usedInitializedInstances;
        }
        DetectCycle();
        
        // From here on we can assume no cycles

        var unorderedList = Initializations.ToList();
        var doneNodes = new HashSet<IInitializedInstanceNode>();
        var orderedList = new List<(IFunctionCallNode, IInitializedInstanceNode)>();
        while (doneNodes.Count != Initializations.Count)
        {
            var nextBatch = unorderedList
                .Where(i => map[i.Item2].All(x => doneNodes.Contains(x)))
                .ToList();
            orderedList.AddRange(nextBatch);
            foreach (var tuple in nextBatch)
            {
                unorderedList.Remove(tuple);
                doneNodes.Add(tuple.Item2);
            }
        }

        Initializations = orderedList;

        IEnumerable<IFunctionNode> SelfAndCalledFunctions(IFunctionNode self)
        {
            yield return self;
            foreach (var calledFunction in self.CalledFunctionsOfSameRange)
                foreach (var function in SelfAndCalledFunctions(calledFunction))
                    yield return function;
        }
        
        void DetectCycle()
        {
            Queue<IInitializedInstanceNode> roots = new(Initializations.Select(i => i.Item2));
            HashSet<IInitializedInstanceNode> v = [];
            HashSet<IInitializedInstanceNode> cf = [];
            Stack<IInitializedInstanceNode> s = new();

            while (roots.Count != 0 && roots.Dequeue() is {} next)
                DetectCycleInner(
                    next, 
                    v, 
                    s,
                    cf);

            void DetectCycleInner(
                IInitializedInstanceNode current, 
                ISet<IInitializedInstanceNode> visited, 
                Stack<IInitializedInstanceNode> stack,
                ISet<IInitializedInstanceNode> cycleFree)
            {
                if (cycleFree.Contains(current))
                    return; // one of the previous roots checked this node already
                if (visited.Contains(current))
                {
                    var cycleStack = ImmutableStack.Create(current.TypeFullName);
                    IInitializedInstanceNode i;
                    do
                    {
                        i = stack.Pop();
                        cycleStack = cycleStack.Push(i.TypeFullName);
                    } while (i != current && stack.Count != 0);
                    
                    _localDiagLogger.Error(
                        ErrorLogData.CircularReferenceAmongInitializedInstances(cycleStack),
                        Location.None);
                    throw new InitializedInstanceCycleDieException(cycleStack);
                }
                visited.Add(current);
                stack.Push(current);
                foreach (var neighbor in map[current])
                    DetectCycleInner(neighbor, visited, stack, cycleFree);
                cycleFree.Add(current);
                stack.Pop();
                visited.Remove(current);
            }
        }
    }
}