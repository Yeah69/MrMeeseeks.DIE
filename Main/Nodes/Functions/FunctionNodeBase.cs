using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal abstract class FunctionNodeBase : IFunctionNode
{
    private readonly ITypeSymbol _typeSymbol;
    private readonly IContainerNode _parentContainer;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<string? , IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode> _plainFunctionCallNodeFactory;
    private readonly Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> _scopeCallNodeFactory;
    private readonly Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> _transientScopeCallNodeFactory;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly List<IPotentiallyAwaitedNode> _potentiallyAwaitingNodes = new();
    private readonly Dictionary<IPotentiallyAwaitedNode, ITaskNodeBase> _asyncWrappingMap = new();
    private readonly List<(IOnAwait, IFunctionCallNode)> _calls = new();
    private readonly List<ILocalFunctionNode> _localFunctions = new();

    private bool _synchronicityCheckedAlready;

    public FunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)> closureParameters,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        IReferenceGenerator referenceGenerator,
        
        // dependencies
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        WellKnownTypes wellKnownTypes)
    {
        _typeSymbol = typeSymbol;
        _parentContainer = parentContainer;
        _referenceGenerator = referenceGenerator;
        _plainFunctionCallNodeFactory = plainFunctionCallNodeFactory;
        _scopeCallNodeFactory = scopeCallNodeFactory;
        _transientScopeCallNodeFactory = transientScopeCallNodeFactory;
        _wellKnownTypes = wellKnownTypes;
        Parameters = parameters.Select(p=> (p.FullName(), p.ToTypeKey(), parameterNodeFactory(p, referenceGenerator)
            .EnqueueBuildJobTo(parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty))).ToList();
        Accessibility = accessibility;
        ReturnedTypeFullName = typeSymbol.FullName();

        var setOfProcessedTypes = new HashSet<TypeKey>();

        var currentOverrides = ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>.Empty;
            
        foreach (var (type, typeKey, parameter) in parameters.Zip(Parameters, (t, tuple) => (t, tuple.Item2, tuple.Item3)))
        {
            if (setOfProcessedTypes.Contains(typeKey)
                || type is not INamedTypeSymbol && type is not IArrayTypeSymbol)
                continue;

            setOfProcessedTypes.Add(typeKey);

            currentOverrides = currentOverrides.SetItem(typeKey, (type, parameter));
        }
            
        foreach (var kvp in closureParameters)
        {
            var typeKey = kvp.Key;
                
            if (setOfProcessedTypes.Contains(typeKey)
                || kvp.Value.Item1 is not INamedTypeSymbol && kvp.Value.Item1 is not IArrayTypeSymbol)
                continue;

            setOfProcessedTypes.Add(typeKey);

            currentOverrides = currentOverrides.SetItem(typeKey, kvp.Value);
        }

        Overrides = currentOverrides;

        RangeFullName = parentRange.FullName;
        DisposedPropertyReference = parentRange.DisposalHandling.DisposedPropertyReference;
    }

    public abstract void Build(ImmutableStack<INamedTypeSymbol> implementationStack);

    public abstract void Accept(INodeVisitor nodeVisitor);

    public ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)> Overrides { get; protected set; }

    public void RegisterAsyncWrapping(IPotentiallyAwaitedNode potentiallyAwaitedNode, ITaskNodeBase taskNodeBase)
    {
        _asyncWrappingMap[potentiallyAwaitedNode] = taskNodeBase;
    }

    public string Description => 
        $"{ReturnedTypeFullName} {RangeFullName}.{Name}({string.Join(", ", Parameters.Select(p => $"{p.Item3.TypeFullName} {p.Item3.Reference}"))})";

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
        _parentContainer.AsyncCheckQueue.Enqueue(this);
    }

    protected virtual void OnBecameAsync() {}

    public void CheckSynchronicity()
    {
        if (_synchronicityCheckedAlready) return;

        if (_potentiallyAwaitingNodes.Any(pan => pan.Awaited))
            ForceToAsync();
    }

    public void ForceToAsync()
    {
        if (SuppressAsync) return;
        _synchronicityCheckedAlready = true;
        if (SynchronicityDecision == SynchronicityDecision.AsyncValueTask) return; 
        SynchronicityDecision = SynchronicityDecision.AsyncValueTask;
        AsyncTypeFullName = _typeSymbol.FullName();
        ReturnedTypeFullName = _wellKnownTypes.ValueTask1.Construct(_typeSymbol).FullName();
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
                Parameters.Select(t => (t.Item3, callingFunction.Overrides[t.Item2].Item2)).ToList(),
                _referenceGenerator)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        _calls.Add((onAwait, call));
        callingFunction.RegisterCalledFunction(this);

        return call;
    }

    public IScopeCallNode CreateScopeCall(string containerParameter, string transientScopeInterfaceParameter, IRangeNode callingRange, IFunctionNode callingFunction, IScopeNode scope)
    {
        var call = _scopeCallNodeFactory(
                containerParameter,
                transientScopeInterfaceParameter,
                scope,
                callingRange,
                this,
                Parameters.Select(t => (t.Item3, callingFunction.Overrides[t.Item2].Item2)).ToList(),
                _referenceGenerator)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
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
                _parentContainer,
                callingRange,
                this,
                Parameters.Select(t => (t.Item3, callingFunction.Overrides[t.Item2].Item2)).ToList(),
                _referenceGenerator)
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        
        _calls.Add((callingFunction, call));
        callingFunction.RegisterCalledFunction(this);

        return call;
    }

    public void AddLocalFunction(ILocalFunctionNode function) =>
        _localFunctions.Add(function);

    public string? ExplicitInterfaceFullName { get; protected set; }
    public IReadOnlyList<ILocalFunctionNode> LocalFunctions => _localFunctions;

    public Accessibility? Accessibility { get; }
    public SynchronicityDecision SynchronicityDecision { get; private set; } = SynchronicityDecision.Sync;
    public abstract string Name { get; protected set; }
    public string ReturnedTypeFullName { get; private set; }

    public IReadOnlyList<(string, TypeKey, IParameterNode)> Parameters { get; protected set; }
}