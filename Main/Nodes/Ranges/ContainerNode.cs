using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.Delegates;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IContainerNode : IRangeNode
{
    string Namespace { get; }
    Queue<BuildJob> BuildQueue { get; }
    Queue<IFunctionNode> AsyncCheckQueue { get; }
    IReadOnlyList<IEntryFunctionNode> RootFunctions { get; }
    IEnumerable<IScopeNode> Scopes { get; }
    IEnumerable<ITransientScopeNode> TransientScopes { get; }
    ITransientScopeInterfaceNode TransientScopeInterface { get; }
    string TransientScopeDisposalReference { get; }
    string TransientScopeDisposalElement { get; }
    IFunctionCallNode BuildContainerInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction);
    IReadOnlyList<ICreateContainerFunctionNode> CreateContainerFunctions { get; }
    bool GenerateEmptyConstructor { get; }

    void RegisterDelegateBaseNode(IDelegateBaseNode delegateBaseNode);
}

internal sealed record BuildJob(INode Node, PassedContext PassedContext);

internal sealed partial class ContainerNode : RangeNode, IContainerNode, IContainerInstance
{
    private readonly IContainerInfo _containerInfo;
    private readonly IFunctionCycleTracker _functionCycleTracker;
    private readonly ICurrentExecutionPhaseSetter _currentExecutionPhaseSetter;
    private readonly Lazy<ITransientScopeInterfaceNode> _lazyTransientScopeInterfaceNode;
    private readonly Func<ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, IEntryFunctionNodeRoot> _entryFunctionNodeFactory;
    private readonly Func<IMethodSymbol?, IVoidFunctionNode?, ICreateContainerFunctionNode> _creatContainerFunctionNodeFactory;
    private readonly List<IEntryFunctionNode> _rootFunctions = [];
    private readonly Lazy<IScopeManager> _lazyScopeManager;
    private readonly Lazy<DisposalType> _lazyDisposalType;
    private readonly List<IDelegateBaseNode> _delegateBaseNodes = [];

    public override string FullName { get; }
    public override DisposalType DisposalType => _lazyDisposalType.Value;
    public string Namespace { get; }
    public Queue<BuildJob> BuildQueue { get; } = new();
    public Queue<IFunctionNode> AsyncCheckQueue { get; } = new();
    public IReadOnlyList<IEntryFunctionNode> RootFunctions => _rootFunctions;
    public IEnumerable<IScopeNode> Scopes => ScopeManager.Scopes;
    public IEnumerable<ITransientScopeNode> TransientScopes => ScopeManager.TransientScopes;
    public ITransientScopeInterfaceNode TransientScopeInterface => _lazyTransientScopeInterfaceNode.Value;
    public string TransientScopeDisposalReference { get; }
    public string TransientScopeDisposalElement { get; }

    public IFunctionCallNode BuildContainerInstanceCall(
        string? ownerReference, 
        INamedTypeSymbol type,
        IFunctionNode callingFunction) =>
        BuildRangedInstanceCall(ownerReference, type, callingFunction, ScopeLevel.Container);

    public IReadOnlyList<ICreateContainerFunctionNode> CreateContainerFunctions { get; private set; } = null!;
    public bool GenerateEmptyConstructor { get; }

    public void RegisterDelegateBaseNode(IDelegateBaseNode delegateBaseNode) => 
        _delegateBaseNodes.Add(delegateBaseNode);

    internal ContainerNode(
        IContainerInfoContext containerInfoContext,
        Func<(INamedTypeSymbol?, INamedTypeSymbol), IUserDefinedElements> userDefinedElementsFactory,
        IReferenceGenerator referenceGenerator,
        IFunctionCycleTracker functionCycleTracker,
        IMapperDataToFunctionKeyTypeConverter mapperDataToFunctionKeyTypeConverter,
        ITypeParameterUtility typeParameterUtility,
        IContainerWideContext containerWideContext,
        ICurrentExecutionPhaseSetter currentExecutionPhaseSetter,
        Lazy<ITransientScopeInterfaceNode> lazyTransientScopeInterfaceNode,
        Lazy<IScopeManager> lazyScopeManager,
        Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueFunctionNodeRoot> multiKeyValueFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueMultiFunctionNodeRoot> multiKeyValueMultiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, IEntryFunctionNodeRoot> entryFunctionNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory,
        Func<IMethodSymbol?, IVoidFunctionNode?, ICreateContainerFunctionNode> creatContainerFunctionNodeFactory)
        : base (
            containerInfoContext.ContainerInfo.Name, 
            containerInfoContext.ContainerInfo.ContainerType,
            userDefinedElementsFactory((containerInfoContext.ContainerInfo.ContainerType, containerInfoContext.ContainerInfo.ContainerType)), 
            mapperDataToFunctionKeyTypeConverter,
            typeParameterUtility,
            containerWideContext,
            referenceGenerator,
            createFunctionNodeFactory,  
            multiFunctionNodeFactory,
            multiKeyValueFunctionNodeFactory,
            multiKeyValueMultiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            voidFunctionNodeFactory,
            disposalHandlingNodeFactory,
            initializedInstanceNodeFactory)
    {
        _containerInfo = containerInfoContext.ContainerInfo;
        _functionCycleTracker = functionCycleTracker;
        _currentExecutionPhaseSetter = currentExecutionPhaseSetter;
        _lazyTransientScopeInterfaceNode = lazyTransientScopeInterfaceNode;
        _entryFunctionNodeFactory = entryFunctionNodeFactory;
        _creatContainerFunctionNodeFactory = creatContainerFunctionNodeFactory;
        Namespace = _containerInfo.Namespace;
        FullName = _containerInfo.FullName;
        
        _lazyScopeManager = lazyScopeManager;
        _lazyDisposalType = new(() => _lazyScopeManager.Value
            .Scopes.Select(s => s.DisposalHandling)
            .Concat(_lazyScopeManager.Value.TransientScopes.Select(ts => ts.DisposalHandling))
            .Prepend(DisposalHandling)
            .Aggregate(DisposalType.None, (agg, next) =>
            {
                if (next.HasSyncDisposables) agg |= DisposalType.Sync;
                if (next.HasAsyncDisposables) agg |= DisposalType.Async;
                return agg;
            }));
        
        TransientScopeDisposalReference = referenceGenerator.Generate("transientScopeDisposal");
        TransientScopeDisposalElement = referenceGenerator.Generate("transientScopeToDispose");
        
        GenerateEmptyConstructor = !_containerInfo.ContainerType.InstanceConstructors.Any(ic => !ic.IsImplicitlyDeclared);
    }

    protected override IScopeManager ScopeManager => _lazyScopeManager.Value;
    protected override IContainerNode ParentContainer => this;
    protected override string ContainerParameterForScope =>
        Constants.ThisKeyword;

    public override void Build(PassedContext passedContext)
    {
        var initializedInstancesFunction = InitializedInstances.Any()
            ? VoidFunctionNodeFactory(
                    InitializedInstances.ToList(),
                    Array.Empty<ITypeSymbol>())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null))
            : null;
        
        if (initializedInstancesFunction is not null)
            _initializationFunctions.Add(initializedInstancesFunction);
        
        var userDefinedConstructors = _containerInfo.ContainerType.InstanceConstructors
            .Where(ic => !ic.IsImplicitlyDeclared)
            .ToList();
        
        CreateContainerFunctions = userDefinedConstructors.Count != 0 
            ? userDefinedConstructors
                .Select(ic => _creatContainerFunctionNodeFactory(ic, initializedInstancesFunction))
                .ToList()
            : [_creatContainerFunctionNodeFactory(null, initializedInstancesFunction)];

        TransientScopeInterface.RegisterRange(this);
        base.Build(passedContext);
        foreach (var (typeSymbol, methodNamePrefix, parameterTypes) in _containerInfo.CreateFunctionData)
        {
            var functionNode = _entryFunctionNodeFactory(
                TypeParameterUtility.ReplaceTypeParametersByCustom(typeSymbol.OriginalDefinitionIfUnbound()),
                methodNamePrefix,
                parameterTypes)
                .Function;
            _rootFunctions.Add(functionNode);
            BuildQueue.Enqueue(new(functionNode, passedContext));
        }

        var asyncCallNodes = new List<IWrappedAsyncFunctionCallNode>();
        while (BuildQueue.Count != 0 && BuildQueue.Dequeue() is { } buildJob)
        {
            buildJob.Node.Build(buildJob.PassedContext);
            if (buildJob.Node is IWrappedAsyncFunctionCallNode call)
                asyncCallNodes.Add(call);
        }

        _currentExecutionPhaseSetter.Value = ExecutionPhase.ResolutionValidation;
        
        while (AsyncCheckQueue.Count != 0 && AsyncCheckQueue.Dequeue() is { } function)
            function.CheckSynchronicity();
        
        foreach (var call in asyncCallNodes)
            call.AdjustToCurrentCalledFunctionSynchronicity();
        
        AdjustRangedInstancesIfGeneric();
        foreach (var scope in Scopes)
            scope.AdjustRangedInstancesIfGeneric();
        foreach (var transientScope in TransientScopes)
            transientScope.AdjustRangedInstancesIfGeneric();
        
        _functionCycleTracker.DetectCycle(this);
        
        CycleDetectionAndReorderingOfInitializedInstances();
        foreach (var scope in Scopes)
            scope.CycleDetectionAndReorderingOfInitializedInstances();
        foreach (var transientScope in TransientScopes)
            transientScope.CycleDetectionAndReorderingOfInitializedInstances();
        
        foreach (var delegateBaseNode in _delegateBaseNodes)
            delegateBaseNode.CheckSynchronicity();
    }

    public override string? ContainerReference => null;

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        BuildContainerInstanceCall(null, type, callingFunction);

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        TransientScopeInterface.BuildTransientScopeInstanceCall($"({Constants.ThisKeyword} as {TransientScopeInterface.FullName})", type, callingFunction);
}