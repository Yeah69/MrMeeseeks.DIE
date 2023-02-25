using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

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
    ITaskTransformationFunctions TaskTransformationFunctions { get; }
    string TransientScopeDisposalReference { get; }
    string TransientScopeDisposalElement { get; }
    IFunctionCallNode BuildContainerInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction);
}

internal record BuildJob(INode Node, ImmutableStack<INamedTypeSymbol> PreviousImplementations);

internal class ContainerNode : RangeNode, IContainerNode, IContainerInstance
{
    private readonly IContainerInfo _containerInfo;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IFunctionCycleTracker _functionCycleTracker;
    private readonly Func<ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IEntryFunctionNode> _entryFunctionNodeFactory;
    private readonly List<IEntryFunctionNode> _rootFunctions = new();
    private readonly Lazy<IScopeManager> _lazyScopeManager;
    private readonly Lazy<DisposalType> _lazyDisposalType;

    public override string FullName { get; }
    public override DisposalType DisposalType => _lazyDisposalType.Value;
    public string Namespace { get; }
    public Queue<BuildJob> BuildQueue { get; } = new();
    public Queue<IFunctionNode> AsyncCheckQueue { get; } = new();
    public IReadOnlyList<IEntryFunctionNode> RootFunctions => _rootFunctions;
    public IEnumerable<IScopeNode> Scopes => ScopeManager.Scopes;
    public IEnumerable<ITransientScopeNode> TransientScopes => ScopeManager.TransientScopes;
    public ITransientScopeInterfaceNode TransientScopeInterface { get; }
    public ITaskTransformationFunctions TaskTransformationFunctions { get; }
    public string TransientScopeDisposalReference { get; }
    public string TransientScopeDisposalElement { get; }

    public IFunctionCallNode BuildContainerInstanceCall(
        string? ownerReference, 
        INamedTypeSymbol type,
        IFunctionNode callingFunction) =>
        BuildRangedInstanceCall(ownerReference, type, callingFunction, ScopeLevel.Container);

    internal ContainerNode(
        IContainerInfo containerInfo,
        ImmutableList<ITypesFromAttributesBase> typesFromAttributesList,
        Func<INamedTypeSymbol, INamedTypeSymbol, IUserDefinedElements> userDefinedElementsFactory,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        IFunctionCycleTracker functionCycleTracker,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IMultiFunctionNode> multiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<ITypeSymbol, string, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IEntryFunctionNode> entryFunctionNodeFactory,
        Func<IContainerNode, IReferenceGenerator, ITransientScopeInterfaceNode> transientScopeInterfaceNodeFactory,
        Func<IReferenceGenerator, ITaskTransformationFunctions> taskTransformationFunctions,
        Func<IContainerInfo, IContainerNode, ITransientScopeInterfaceNode, ImmutableList<ITypesFromAttributesBase>, IReferenceGenerator, IScopeManager> scopeManagerFactory,
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory)
        : base (
            containerInfo.Name, 
            userDefinedElementsFactory(containerInfo.ContainerType, containerInfo.ContainerType), 
            checkTypeProperties,
            referenceGenerator, 
            createFunctionNodeFactory,  
            multiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            disposalHandlingNodeFactory)
    {
        _containerInfo = containerInfo;
        _referenceGenerator = referenceGenerator;
        _functionCycleTracker = functionCycleTracker;
        _entryFunctionNodeFactory = entryFunctionNodeFactory;
        Namespace = containerInfo.Namespace;
        FullName = _containerInfo.FullName;
        
        TransientScopeInterface = transientScopeInterfaceNodeFactory(this, referenceGenerator);
        _lazyScopeManager = new(() => scopeManagerFactory(
            containerInfo, this, TransientScopeInterface, typesFromAttributesList, referenceGenerator));
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

        TaskTransformationFunctions = taskTransformationFunctions(referenceGenerator);
        
        TransientScopeDisposalReference = referenceGenerator.Generate("transientScopeDisposal");
        TransientScopeDisposalElement = referenceGenerator.Generate("transientScopeToDispose");
    }

    protected override IScopeManager ScopeManager => _lazyScopeManager.Value;
    protected override IContainerNode ParentContainer => this;
    protected override string ContainerParameterForScope =>
        Constants.ThisKeyword;

    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        TransientScopeInterface.RegisterRange(this);
        base.Build(implementationStack);
        foreach (var (typeSymbol, methodNamePrefix, parameterTypes) in _containerInfo.CreateFunctionData)
        {
            var functionNode = _entryFunctionNodeFactory(
                typeSymbol,
                methodNamePrefix,
                parameterTypes,
                this,
                this,
                UserDefinedElements,
                CheckTypeProperties,
                _referenceGenerator);
            _rootFunctions.Add(functionNode);
            BuildQueue.Enqueue(new(functionNode, implementationStack));
        }

        while (BuildQueue.Any() && BuildQueue.Dequeue() is { } buildJob) 
            buildJob.Node.Build(buildJob.PreviousImplementations);
        
        while (AsyncCheckQueue.Any() && AsyncCheckQueue.Dequeue() is { } function)
            function.CheckSynchronicity();
        
        _functionCycleTracker.DetectCycle(this);
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitContainerNode(this);

    public override string? ContainerReference => null;

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        BuildContainerInstanceCall(null, type, callingFunction);

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        TransientScopeInterface.BuildTransientScopeInstanceCall($"({Constants.ThisKeyword} as {TransientScopeInterface.FullName})", type, callingFunction);
}