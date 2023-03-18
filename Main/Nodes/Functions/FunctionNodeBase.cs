using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal abstract class FunctionNodeBase : IFunctionNode
{
    private readonly IContainerNode _parentContainer;
    private readonly Func<string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode> _plainFunctionCallNodeFactory;
    private readonly Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IAsyncFunctionCallNode> _asyncFunctionCallNodeFactory;
    private readonly Func<(string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> _scopeCallNodeFactory;
    private readonly Func<string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> _transientScopeCallNodeFactory;
    protected readonly WellKnownTypes WellKnownTypes;
    private readonly List<IAwaitableNode> _awaitableNodes = new();
    private readonly List<ILocalFunctionNode> _localFunctions = new();
    private readonly List<IFunctionNode> _callingFunctions = new();

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
        Func<string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        IContainerWideContext containerWideContext)
    {
        _parentContainer = parentContainer;
        _plainFunctionCallNodeFactory = plainFunctionCallNodeFactory;
        _asyncFunctionCallNodeFactory = asyncFunctionCallNodeFactory;
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

    public virtual void Build(ImmutableStack<INamedTypeSymbol> implementationStack) =>
        _parentContainer.AsyncCheckQueue.Enqueue(this);

    public abstract void Accept(INodeVisitor nodeVisitor);

    public ImmutableDictionary<ITypeSymbol, IParameterNode> Overrides { get; }

    public string Description => 
        $"{ReturnedTypeFullName} {RangeFullName}.{Name}({string.Join(", ", Parameters.Select(p => $"{p.Node.TypeFullName} {p.Node.Reference}"))})";

    public HashSet<IFunctionNode> CalledFunctions { get; } = new ();

    public void RegisterAwaitableNode(IAwaitableNode awaitableNode) => 
        _awaitableNodes.Add(awaitableNode);

    public void RegisterCalledFunction(IFunctionNode calledFunction) => 
        CalledFunctions.Add(calledFunction);

    public void RegisterCallingFunction(IFunctionNode callingFunction)
    {
        _callingFunctions.Add(callingFunction);
    }

    protected virtual void OnBecameAsync() {}

    public void CheckSynchronicity()
    {
        if (_synchronicityCheckedAlready) return;
        
        if (_awaitableNodes.Any(an => an.Awaited))
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
        OnBecameAsync();
        foreach (var callingFunction in _callingFunctions)
            _parentContainer.AsyncCheckQueue.Enqueue(callingFunction);
    }

    protected virtual bool SuppressAsync => false;

    public string? AsyncTypeFullName { get; private set; }
    public string RangeFullName { get; }
    public string DisposedPropertyReference { get; }

    public IFunctionCallNode CreateCall(string? ownerReference, IFunctionNode callingFunction)
    {
        var call = _plainFunctionCallNodeFactory(
                ownerReference,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList())
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

        return call;
    }

    public IAsyncFunctionCallNode CreateAsyncCall(ITypeSymbol wrappedType, string? ownerReference, SynchronicityDecision synchronicity, IFunctionNode callingFunction)
    {
        var call = _asyncFunctionCallNodeFactory(
                wrappedType,
                ownerReference,
                synchronicity,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList())
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

        return call;
    }

    public IScopeCallNode CreateScopeCall(string containerParameter, string transientScopeInterfaceParameter, IRangeNode callingRange, IFunctionNode callingFunction, IScopeNode scope)
    {
        var call = _scopeCallNodeFactory(
                (containerParameter, transientScopeInterfaceParameter),
                scope,
                callingRange,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                scope.InitializedInstances.Any() ? scope.BuildInitializationCall(callingFunction) : null)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

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
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                transientScopeNode.InitializedInstances.Any() ? transientScopeNode.BuildInitializationCall(callingFunction) : null)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

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