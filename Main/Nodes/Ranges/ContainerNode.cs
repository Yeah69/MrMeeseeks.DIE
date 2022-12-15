using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IContainerNode : IRangeNode
{
    string Namespace { get; }
    Queue<INode> BuildQueue { get; }
    Queue<IFunctionNode> AsyncCheckQueue { get; }
    IReadOnlyList<IEntryFunctionNode> RootFunctions { get; }
    IEnumerable<IScopeNode> Scopes { get; }
    IEnumerable<ITransientScopeNode> TransientScopes { get; }
    ITransientScopeInterfaceNode TransientScopeInterface { get; }
    IFunctionCallNode BuildContainerInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction);
}

internal class ContainerNode : RangeNode, IContainerNode
{
    private readonly IContainerInfo _containerInfo;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<ITypeSymbol, string, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IEntryFunctionNode> _entryFunctionNodeFactory;
    private readonly List<IEntryFunctionNode> _rootFunctions = new();
    private readonly Lazy<IScopeManager> _lazyScopeManager;

    public override string FullName { get; }
    public string Namespace { get; }
    public Queue<INode> BuildQueue { get; } = new();
    public Queue<IFunctionNode> AsyncCheckQueue { get; } = new();
    public IReadOnlyList<IEntryFunctionNode> RootFunctions => _rootFunctions;
    public IEnumerable<IScopeNode> Scopes => ScopeManager.Scopes;
    public IEnumerable<ITransientScopeNode> TransientScopes => ScopeManager.TransientScopes;
    public ITransientScopeInterfaceNode TransientScopeInterface { get; }

    public IFunctionCallNode BuildContainerInstanceCall(
        string? ownerReference, 
        INamedTypeSymbol type,
        IFunctionNode callingFunction) =>
        BuildRangedInstanceCall(ownerReference, type, callingFunction, ScopeLevel.Container);

    internal ContainerNode(
        IContainerInfo containerInfo,
        ImmutableList<ITypesFromAttributes> typesFromAttributesList,
        Func<INamedTypeSymbol, INamedTypeSymbol, IUserDefinedElements> userDefinedElementsFactory,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> createFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<ITypeSymbol, string, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IEntryFunctionNode> entryFunctionNodeFactory,
        Func<IContainerNode, IReferenceGenerator, ITransientScopeInterfaceNode> transientScopeInterfaceNodeFactory,
        Func<
            IContainerInfo,
            IContainerNode,
            ITransientScopeInterfaceNode,
            ImmutableList<ITypesFromAttributes>,
            IReferenceGenerator, 
            IScopeManager> scopeManagerFactory)
        : base (
            containerInfo.Name, 
            userDefinedElementsFactory(containerInfo.ContainerType, containerInfo.ContainerType), 
            checkTypeProperties,
            referenceGenerator, 
            createFunctionNodeFactory, 
            rangedInstanceFunctionGroupNodeFactory)
    {
        _containerInfo = containerInfo;
        _referenceGenerator = referenceGenerator;
        _entryFunctionNodeFactory = entryFunctionNodeFactory;
        Namespace = containerInfo.Namespace;
        FullName = _containerInfo.FullName;
        
        TransientScopeInterface = transientScopeInterfaceNodeFactory(this, referenceGenerator);
        _lazyScopeManager = new Lazy<IScopeManager>(() => scopeManagerFactory(
            containerInfo, this, TransientScopeInterface, typesFromAttributesList, referenceGenerator));
    }

    protected override IScopeManager ScopeManager => _lazyScopeManager.Value;
    protected override IContainerNode ParentContainer => this;
    protected override string ContainerParameterForScope =>
        Constants.ThisKeyword;

    public override void Build()
    {
        TransientScopeInterface.RegisterRange(this);
        base.Build();
        foreach (var (typeSymbol, methodNamePrefix) in _containerInfo.CreateFunctionData)
        {
            var functionNode = _entryFunctionNodeFactory(
                typeSymbol,
                methodNamePrefix,
                this,
                this,
                UserDefinedElements,
                CheckTypeProperties,
                _referenceGenerator);
            _rootFunctions.Add(functionNode);
            BuildQueue.Enqueue(functionNode);
        }

        while (BuildQueue.Any() && BuildQueue.Dequeue() is { } node) 
            node.Build();
        
        while (AsyncCheckQueue.Any() && AsyncCheckQueue.Dequeue() is { } function)
            function.CheckSynchronicity();
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitContainerNode(this);

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        BuildContainerInstanceCall(null, type, callingFunction);

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        TransientScopeInterface.BuildTransientScopeInstanceCall($"({Constants.ThisKeyword} as {TransientScopeInterface.FullName})", type, callingFunction);
}