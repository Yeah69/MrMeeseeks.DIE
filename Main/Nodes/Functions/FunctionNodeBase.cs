using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal abstract class FunctionNodeBase : IFunctionNode
{
    private readonly IContainerNode _parentContainer;
    private readonly Lazy<IFunctionNodeGenerator> _functionNodeGenerator;
    private readonly Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> _plainFunctionCallNodeFactory;
    private readonly Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> _asyncFunctionCallNodeFactory;
    private readonly Func<ScopeCallNode.Params, IScopeCallNode> _scopeCallNodeFactory;
    private readonly Func<TransientScopeCallNode.Params, ITransientScopeCallNode> _transientScopeCallNodeFactory;
    protected readonly WellKnownTypes WellKnownTypes;
    private readonly List<IAwaitableNode> _awaitableNodes = [];
    private readonly List<ILocalFunctionNode> _localFunctions = [];
    private readonly List<IFunctionNode> _callingFunctions = [];
    private readonly List<IInitializedInstanceNode> _usedInitializedInstances = [];

    private readonly Dictionary<ITypeSymbol, IReusedNode> _reusedNodes =
        new(CustomSymbolEqualityComparer.IncludeNullability);

    private bool _synchronicityCheckedAlready;

    protected FunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
        
        // dependencies
        IContainerNode parentContainer,
        IRangeNode parentRange,
        ISubDisposalNodeChooser subDisposalNodeChooser,
        ITransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        WellKnownTypes wellKnownTypes)
    {
        _parentContainer = parentContainer;
        _functionNodeGenerator = functionNodeGenerator;
        _plainFunctionCallNodeFactory = plainFunctionCallNodeFactory;
        _asyncFunctionCallNodeFactory = asyncFunctionCallNodeFactory;
        _scopeCallNodeFactory = scopeCallNodeFactory;
        _transientScopeCallNodeFactory = transientScopeCallNodeFactory;
        WellKnownTypes = wellKnownTypes;
        Parameters = parameters.Select(p=> (p, parameterNodeFactory(p)
            .EnqueueBuildJobTo(parentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)))).ToList();
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
        SubDisposalNode = subDisposalNodeChooser.ChooseSubDisposalNode();
        TransientScopeDisposalNode = transientScopeDisposalNodeChooser.ChooseTransientScopeDisposalNode();
    }

    public virtual void Build(PassedContext passedContext) =>
        _parentContainer.AsyncCheckQueue.Enqueue(this);

    public abstract void Accept(INodeVisitor nodeVisitor);

    public ImmutableDictionary<ITypeSymbol, IParameterNode> Overrides { get; }
    public virtual IReadOnlyList<ITypeParameterSymbol> TypeParameters { get; } = Array.Empty<ITypeParameterSymbol>();

    public string Description => 
        $"{ReturnedTypeFullName} {RangeFullName}.{Name}({string.Join(", ", Parameters.Select(p => $"{p.Node.TypeFullName} {p.Node.Reference}"))})";

    public HashSet<IFunctionNode> CalledFunctions { get; } = [];

    public IEnumerable<IFunctionNode> CalledFunctionsOfSameRange =>
        CalledFunctions.Where(cf => Equals(cf.RangeFullName, RangeFullName));

    public IEnumerable<IInitializedInstanceNode> UsedInitializedInstance => _usedInitializedInstances;

    public void RegisterAwaitableNode(IAwaitableNode awaitableNode) => 
        _awaitableNodes.Add(awaitableNode);

    public void RegisterCalledFunction(IFunctionNode calledFunction) => 
        CalledFunctions.Add(calledFunction);

    public void RegisterCallingFunction(IFunctionNode callingFunction) => 
        _callingFunctions.Add(callingFunction);

    public void RegisterUsedInitializedInstance(IInitializedInstanceNode initializedInstance) => 
        _usedInitializedInstances.Add(initializedInstance);

    private int _subDisposalCount;
    public void AddOneToSubDisposalCount() => _subDisposalCount++;

    public int GetSubDisposalCount() =>
        _subDisposalCount +
        CalledFunctions
            .Where(f => f.IsSubDisposalAsParameter)
            .Sum(f => f.GetSubDisposalCount());

    private int _transientScopeDisposalCount;
    public void AddOneToTransientScopeDisposalCount() => _transientScopeDisposalCount++;

    public int GetTransientScopeDisposalCount() =>
        _transientScopeDisposalCount +
        CalledFunctions
            .Where(f => f.IsTransientScopeDisposalAsParameter)
            .Sum(f => f.GetTransientScopeDisposalCount());

    protected virtual void OnBecameAsync() {}

    public void CheckSynchronicity()
    {
        if (_synchronicityCheckedAlready) return;
        
        if (_awaitableNodes.Any(an => an.Awaited))
            ForceToAsync();
    }

    protected abstract void AdjustToAsync();

    public void ForceToAsync()
    {
        if (SuppressAsync) return;
        _synchronicityCheckedAlready = true;
        if (SynchronicityDecisionKind != SynchronicityDecisionKind.Sync) return; // Already async
        SynchronicityDecisionKind = SynchronicityDecisionKind.AsyncForced;
        AdjustToAsync();
        OnBecameAsync();
        foreach (var callingFunction in _callingFunctions)
            _parentContainer.AsyncCheckQueue.Enqueue(callingFunction);
    }

    protected virtual bool SuppressAsync => false;
    public string RangeFullName { get; }
    public IElementNode SubDisposalNode { get; }
    public IElementNode TransientScopeDisposalNode { get; }
    public bool IsSubDisposalAsParameter => SubDisposalNode is IParameterNode;
    public bool IsTransientScopeDisposalAsParameter => TransientScopeDisposalNode is IParameterNode;

    public IFunctionCallNode CreateCall(
        ITypeSymbol callSideType,
        string? ownerReference, 
        IFunctionNode callingFunction, 
        IReadOnlyList<ITypeSymbol> typeParameters)
    {
        var call = _plainFunctionCallNodeFactory(
                new PlainFunctionCallNode.Params(
                    callSideType,
                    ownerReference,
                    Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                    typeParameters,
                    callingFunction.SubDisposalNode,
                    callingFunction.TransientScopeDisposalNode))
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

        return call;
    }

    public IWrappedAsyncFunctionCallNode CreateAsyncCall(
        ITypeSymbol wrappedType, 
        string? ownerReference, 
        SynchronicityDecision synchronicity, 
        IFunctionNode callingFunction, 
        IReadOnlyList<ITypeSymbol> typeParameters)
    {
        var call = _asyncFunctionCallNodeFactory(
                new WrappedAsyncFunctionCallNode.Params(
                    wrappedType,
                    ownerReference,
                    synchronicity,
                    Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                    typeParameters,
                    callingFunction.SubDisposalNode,
                    callingFunction.TransientScopeDisposalNode))
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

        return call;
    }

    public IScopeCallNode CreateScopeCall(
        ITypeSymbol callSideType,
        string containerParameter,
        string transientScopeInterfaceParameter,
        IRangeNode callingRange, 
        IFunctionNode callingFunction,
        IScopeNode scope, 
        IReadOnlyList<ITypeSymbol> typeParameters,
        IElementNodeMapperBase scopeImplementationMapper)
    {
        var call = _scopeCallNodeFactory(
                new ScopeCallNode.Params(
                    callSideType,
                    containerParameter,
                    transientScopeInterfaceParameter,
                    scope,
                    callingFunction,
                    Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                    typeParameters,
                    scope.InitializedInstances.Any() ? scope.BuildInitializationCall(callingFunction) : null,
                    new ScopeCallNodeOuterMapperParam(scopeImplementationMapper)))
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

        return call;
    }

    public ITransientScopeCallNode CreateTransientScopeCall(
        ITypeSymbol callSideType,
        string containerParameter, 
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        ITransientScopeNode transientScopeNode,
        IReadOnlyList<ITypeSymbol> typeParameters,
        IElementNodeMapperBase transientScopeImplementationMapper)
    {
        var call = _transientScopeCallNodeFactory(
            new TransientScopeCallNode.Params(
                callSideType,
                containerParameter,
                transientScopeNode,
                callingRange,
                callingFunction,
                callingFunction.TransientScopeDisposalNode,
                Parameters.Select(t => (t.Node, callingFunction.Overrides[t.Type])).ToList(),
                typeParameters,
                transientScopeNode.InitializedInstances.Any() ? transientScopeNode.BuildInitializationCall(callingFunction) : null,
                new ScopeCallNodeOuterMapperParam(transientScopeImplementationMapper)))
            .EnqueueBuildJobTo(_parentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
        
        callingFunction.RegisterCalledFunction(this);
        callingFunction.RegisterAwaitableNode(call);
        _callingFunctions.Add(callingFunction);

        return call;
    }

    public abstract bool CheckIfReturnedType(ITypeSymbol type);
    public bool TryGetReusedNode(ITypeSymbol type, out IReusedNode? reusedNode)
    {
        reusedNode = null;
        if (!_reusedNodes.TryGetValue(type, out var rn))
            return false;
        reusedNode = rn;
        return true;
    }

    public void AddReusedNode(ITypeSymbol type, IReusedNode reusedNode) => 
        _reusedNodes[type] = reusedNode;

    public INodeGenerator GetGenerator() => _functionNodeGenerator.Value;

    public void AddLocalFunction(ILocalFunctionNode function) =>
        _localFunctions.Add(function);

    public string? ExplicitInterfaceFullName { get; protected set; }
    public IReadOnlyList<ILocalFunctionNode> LocalFunctions => _localFunctions;

    public Accessibility? Accessibility { get; }
    public SynchronicityDecision SynchronicityDecision { get; protected set; } = SynchronicityDecision.Sync;
    public SynchronicityDecisionKind SynchronicityDecisionKind { get; protected set; } = SynchronicityDecisionKind.Sync;
    public abstract string Name { get; protected set; }
    public string ReturnedTypeFullName { get; protected set; } = "";
    public abstract string ReturnedTypeNameNotWrapped { get; }

    public IReadOnlyList<(ITypeSymbol Type, IParameterNode Node)> Parameters { get; }
}