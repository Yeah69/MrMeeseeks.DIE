using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal abstract class FunctionNodeBase : IFunctionNode
{
    protected readonly IContainerNode ParentContainer;
    private readonly Func<string? , IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode> _plainFunctionCallNodeFactory;
    private readonly Func<(string, string), IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> _scopeCallNodeFactory;
    private readonly Func<string, ITransientScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> _transientScopeCallNodeFactory;
    protected readonly WellKnownTypes WellKnownTypes;
    private readonly List<IPotentiallyAwaitedNode> _potentiallyAwaitingNodes = new();
    private readonly Dictionary<IPotentiallyAwaitedNode, ITaskNodeBase> _asyncWrappingMap = new();
    private readonly List<(IOnAwait, IFunctionCallNode)> _calls = new();
    private readonly List<ILocalFunctionNode> _localFunctions = new();

    private bool _synchronicityCheckedAlready;

    public FunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        
        // dependencies
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        IContainerWideContext containerWideContext)
    {
        ParentContainer = parentContainer;
        _plainFunctionCallNodeFactory = plainFunctionCallNodeFactory;
        _scopeCallNodeFactory = scopeCallNodeFactory;
        _transientScopeCallNodeFactory = transientScopeCallNodeFactory;
        WellKnownTypes = containerWideContext.WellKnownTypes;
        Parameters = parameters.Select(p=> (p, parameterNodeFactory(p)
            .EnqueueBuildJobTo(parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty))).ToList();
        Accessibility = accessibility;

        var setOfProcessedTypes = new HashSet<ITypeSymbol>(CustomSymbolEqualityComparer.IncludeNullability);

        var currentOverrides = ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability);
            
        foreach (var (type, node) in parameters.Zip(Parameters, (t, tuple) => (t, tuple.Node)))
        {
            if (setOfProcessedTypes.Contains(type)
                || type is not INamedTypeSymbol && type is not IArrayTypeSymbol)
                continue;

            setOfProcessedTypes.Add(type);

            currentOverrides = currentOverrides.SetItem(type, node);
        }
            
        foreach (var kvp in closureParameters)
        {
            if (setOfProcessedTypes.Contains(kvp.Key)
                || kvp.Key is not INamedTypeSymbol && kvp.Key is not IArrayTypeSymbol)
                continue;

            setOfProcessedTypes.Add(kvp.Key);

            currentOverrides = currentOverrides.SetItem(kvp.Key, kvp.Value);
        }

        Overrides = currentOverrides;

        RangeFullName = parentRange.FullName;
        DisposedPropertyReference = parentRange.DisposalHandling.DisposedPropertyReference;
    }

    public abstract void Build(ImmutableStack<INamedTypeSymbol> implementationStack);

    public abstract void Accept(INodeVisitor nodeVisitor);

    public ImmutableDictionary<ITypeSymbol, IParameterNode> Overrides { get; }

    public void RegisterAsyncWrapping(IPotentiallyAwaitedNode potentiallyAwaitedNode, ITaskNodeBase taskNodeBase)
    {
        _asyncWrappingMap[potentiallyAwaitedNode] = taskNodeBase;
    }

    public string Description => 
        $"{ReturnedTypeFullName} {RangeFullName}.{Name}({string.Join(", ", Parameters.Select(p => $"{p.Node.TypeFullName} {p.Node.Reference}"))})";

    public HashSet<IFunctionNode> CalledFunctions { get; } = new ();

    public void RegisterCalledFunction(IFunctionNode calledFunction)
    {
        CalledFunctions.Add(calledFunction);
    }

    public void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode)
    {
        if (_asyncWrappingMap.TryGetValue(potentiallyAwaitedNode, out var taskNodeBase))
            taskNodeBase.OnAwait(potentiallyAwaitedNode);
        _potentiallyAwaitingNodes.Add(potentiallyAwaitedNode);
        _synchronicityCheckedAlready = false;
        ParentContainer.AsyncCheckQueue.Enqueue(this);
    }

    protected virtual void OnBecameAsync() {}

    public void CheckSynchronicity()
    {
        if (_synchronicityCheckedAlready) return;

        if (_potentiallyAwaitingNodes.Any(pan => pan.Awaited))
            ForceToAsync();
    }

    protected abstract string GetAsyncTypeFullName();

    protected abstract string GetReturnedTypeFullName();

    public void ForceToAsync()
    {
        if (SuppressAsync) return;
        _synchronicityCheckedAlready = true;
        if (SynchronicityDecision == SynchronicityDecision.AsyncValueTask) return; 
        SynchronicityDecision = SynchronicityDecision.AsyncValueTask;
        AsyncTypeFullName = GetAsyncTypeFullName();
        ReturnedTypeFullName = GetReturnedTypeFullName();
        foreach (var (callingFunction, call) in _calls)
            call.MakeAsync(callingFunction);
        OnBecameAsync();
    }

    protected virtual bool SuppressAsync => false;

    public string? AsyncTypeFullName { get; private set; }
    public string RangeFullName { get; }
    public string DisposedPropertyReference { get; }

    public IFunctionCallNode CreateCall(string? ownerReference, IFunctionNode callingFunction, IOnAwait onAwait)
    {
        var call = _plainFunctionCallNodeFactory(
                ownerReference,
                this,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList())
            .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        _calls.Add((onAwait, call));
        callingFunction.RegisterCalledFunction(this);

        return call;
    }

    public IScopeCallNode CreateScopeCall(string containerParameter, string transientScopeInterfaceParameter, IRangeNode callingRange, IFunctionNode callingFunction, IScopeNode scope)
    {
        var call = _scopeCallNodeFactory(
                (containerParameter, transientScopeInterfaceParameter),
                scope,
                callingRange,
                this,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                scope.InitializedInstances.Any() ? scope.BuildInitializationCall(callingFunction) : null)
            .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        _calls.Add((callingFunction, call));
        callingFunction.RegisterCalledFunction(this);

        return call;
    }

    public ITransientScopeCallNode CreateTransientScopeCall(
        string containerParameter, 
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        ITransientScopeNode transientScopeNode)
    {
        var call = _transientScopeCallNodeFactory(
                containerParameter,
                transientScopeNode,
                callingRange,
                this,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                transientScopeNode.InitializedInstances.Any() ? transientScopeNode.BuildInitializationCall(callingFunction) : null)
            .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        _calls.Add((callingFunction, call));
        callingFunction.RegisterCalledFunction(this);

        return call;
    }

    public abstract bool CheckIfReturnedType(ITypeSymbol type);

    public void AddLocalFunction(ILocalFunctionNode function) =>
        _localFunctions.Add(function);

    public string? ExplicitInterfaceFullName { get; protected set; }
    public IReadOnlyList<ILocalFunctionNode> LocalFunctions => _localFunctions;

    public Accessibility? Accessibility { get; }
    public SynchronicityDecision SynchronicityDecision { get; private set; } = SynchronicityDecision.Sync;
    public abstract string Name { get; protected set; }
    public string ReturnedTypeFullName { get; protected set; } = "";

    public IReadOnlyList<(ITypeSymbol Type, IParameterNode Node)> Parameters { get; }
}