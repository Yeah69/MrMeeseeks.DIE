using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IVoidFunctionNode : IFunctionNode
{
    IReadOnlyList<(IFunctionCallNode, IInitializedInstanceNode)> Initializations { get; }
    void ReorderOrDetectCycle();
}

internal class VoidFunctionNode : FunctionNodeBase, IVoidFunctionNode, IScopeInstance
{
    private readonly IReadOnlyList<IInitializedInstanceNode> _initializedInstanceNodes;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly IRangeNode _parentRange;

    internal VoidFunctionNode(
        // parameters
        IReadOnlyList<IInitializedInstanceNode> initializedInstanceNodes,
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        ITransientScopeWideContext transientScopeWideContext,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator,
        ILocalDiagLogger localDiagLogger,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        IContainerWideContext containerWideContext)
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal, 
            parameters, 
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentContainer, 
            transientScopeWideContext.Range,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _initializedInstanceNodes = initializedInstanceNodes;
        _localDiagLogger = localDiagLogger;
        _parentRange = transientScopeWideContext.Range;
        ReturnedTypeFullName = "void";
        Name = referenceGenerator.Generate("Initialize");
    }
    
    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        base.Build(implementationStack);
        Initializations = _initializedInstanceNodes
            .Select(i => (i.BuildCall(_parentRange, this), i))
            .ToList();
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitVoidFunctionNode(this);
    protected override string GetAsyncTypeFullName() => "void";

    protected override string GetReturnedTypeFullName() =>
        SynchronicityDecision == SynchronicityDecision.AsyncValueTask 
            ? WellKnownTypes.ValueTask.FullName()
            : WellKnownTypes.Task.FullName();

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
            HashSet<IInitializedInstanceNode> v = new();
            HashSet<IInitializedInstanceNode> cf = new();
            Stack<IInitializedInstanceNode> s = new();

            while (roots.Any() && roots.Dequeue() is {} next)
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
                    } while (i != current && stack.Any());
                    
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