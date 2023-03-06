using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IRangeNode : INode
{
    string FullName { get; }
    string Name { get; }
    DisposalType DisposalType { get; }
    IDisposalHandlingNode DisposalHandling { get; }
    bool AddForDisposal { get; }
    bool AddForDisposalAsync { get; }
    string? ContainerReference { get; }
    IEnumerable<IInitializedInstanceNode> InitializedInstances { get; }

    IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction);
    ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IRangedInstanceFunctionNode BuildTransientScopeFunction(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction, IOnAwait onAwait);
    IEnumerable<ICreateFunctionNodeBase> CreateFunctions { get; }
    IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups { get; }
    IEnumerable<IMultiFunctionNode> MultiFunctions { get; }
    IEnumerable<IVoidFunctionNode> InitializationFunctions { get; }
    IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IInitializedInstanceNode? GetInitializedNode(INamedTypeSymbol type);
    IFunctionCallNode BuildInitializationCall(IFunctionNode callingFunction);
}

internal abstract class RangeNode : IRangeNode
{
    protected readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> CreateFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> _multiFunctionNodeFactory;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodeFactory;
    private readonly Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IVoidFunctionNodeRoot> _voidFunctionNodeFactory;
    protected readonly Dictionary<ITypeSymbol, List<ICreateFunctionNodeBase>> _createFunctions = new(CustomSymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<ITypeSymbol, List<IMultiFunctionNode>> _multiFunctions = new(CustomSymbolEqualityComparer.IncludeNullability);

    private readonly Dictionary<ITypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodes = new(CustomSymbolEqualityComparer.IncludeNullability);
    
    private readonly List<IVoidFunctionNode> _initializationFunctions = new();

    protected readonly Dictionary<INamedTypeSymbol, IInitializedInstanceNode> InitializedInstanceNodesMap = new(CustomSymbolEqualityComparer.IncludeNullability);

    public abstract string FullName { get; }
    public string Name { get; }
    public abstract DisposalType DisposalType { get; }
    public IDisposalHandlingNode DisposalHandling { get; }
    public bool AddForDisposal { get; }
    public bool AddForDisposalAsync { get; }
    public abstract string? ContainerReference { get; }

    public IEnumerable<IInitializedInstanceNode> InitializedInstances => InitializedInstanceNodesMap.Values;

    public IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction, IOnAwait onAwait) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _multiFunctions,
            () => _multiFunctionNodeFactory(
                type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateCall(null, callingFunction, onAwait));

    public IEnumerable<ICreateFunctionNodeBase> CreateFunctions => _createFunctions.Values.SelectMany(l => l);

    public IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups =>
        _rangedInstanceFunctionGroupNodes.Values;

    public IEnumerable<IMultiFunctionNode> MultiFunctions => _multiFunctions.Values.SelectMany(l => l);
    public IEnumerable<IVoidFunctionNode> InitializationFunctions => _initializationFunctions;

    internal RangeNode(
        string name,
        IUserDefinedElementsBase userDefinedElements,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory)
    {
        CreateFunctionNodeFactory = createFunctionNodeFactory;
        _multiFunctionNodeFactory = multiFunctionNodeFactory;
        _rangedInstanceFunctionGroupNodeFactory = rangedInstanceFunctionGroupNodeFactory;
        _voidFunctionNodeFactory = voidFunctionNodeFactory;
        Name = name;

        DisposalHandling = disposalHandlingNodeFactory();

        if (userDefinedElements.AddForDisposal is { })
        {
            AddForDisposal = true;
            DisposalHandling.RegisterSyncDisposal();
        }

        if (userDefinedElements.AddForDisposalAsync is { })
        {
            AddForDisposalAsync = true;
            DisposalHandling.RegisterAsyncDisposal();
        }
    }
    
    protected abstract IScopeManager ScopeManager { get; }
    
    protected abstract IContainerNode ParentContainer { get; }
    
    protected abstract string ContainerParameterForScope { get; }

    protected virtual string TransientScopeInterfaceParameterForScope => Constants.ThisKeyword;

    public virtual void Build(ImmutableStack<INamedTypeSymbol> implementationStack) {}

    public abstract void Accept(INodeVisitor nodeVisitor);
    public IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _createFunctions,
            () => CreateFunctionNodeFactory(
                type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateCall(null, callingFunction, callingFunction));
    
    public IFunctionCallNode BuildInitializationCall(IFunctionNode callingFunction)
    {
        var voidFunction = FunctionResolutionUtility.GetOrCreateFunction(
            callingFunction,
            _initializationFunctions,
            () => _voidFunctionNodeFactory(
                    InitializedInstances.ToList(),
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList(),
                    this,
                    ParentContainer)
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty));

        return voidFunction.CreateCall(null, callingFunction, callingFunction);
    }

    public ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ScopeManager.GetTransientScope(type).BuildTransientScopeCallFunction(ContainerParameterForScope, type, this, callingFunction);

    public IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ScopeManager.GetScope(type).BuildScopeCallFunction(ContainerParameterForScope, TransientScopeInterfaceParameterForScope, type, this, callingFunction);

    public IInitializedInstanceNode? GetInitializedNode(INamedTypeSymbol type) => 
        InitializedInstanceNodesMap.TryGetValue(type, out var initializedInstanceNode) 
            ? initializedInstanceNode
            : null;

    protected IFunctionCallNode BuildRangedInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction, ScopeLevel level)
    {
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(type, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                level,
                type)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
            _rangedInstanceFunctionGroupNodes[type] = rangedInstanceFunctionGroupNode;
        }
        var function = rangedInstanceFunctionGroupNode.BuildFunction(callingFunction);
        return function.CreateCall(ownerReference, callingFunction, callingFunction);
    }

    public abstract IFunctionCallNode BuildContainerInstanceCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction);

    public abstract IFunctionCallNode BuildTransientScopeInstanceCall(
        INamedTypeSymbol type,
        IFunctionNode callingFunction);

    public IRangedInstanceFunctionNode BuildTransientScopeFunction(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction)
    {
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(type, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                ScopeLevel.TransientScope,
                type)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
            _rangedInstanceFunctionGroupNodes[type] = rangedInstanceFunctionGroupNode;
        }
        var function = rangedInstanceFunctionGroupNode.BuildFunction(callingFunction);
        function.CreateCall(null, callingFunction, callingFunction);
        return function;
    }

    public IFunctionCallNode BuildScopeInstanceCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction) =>
        BuildRangedInstanceCall(null, type, callingFunction, ScopeLevel.Scope);
}