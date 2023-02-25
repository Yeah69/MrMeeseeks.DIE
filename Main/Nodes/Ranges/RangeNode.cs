using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
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

    IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction);
    ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IRangedInstanceFunctionNode BuildTransientScopeFunction(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction, IOnAwait onAwait);
    IEnumerable<ICreateFunctionNode> CreateFunctions { get; }
    IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups { get; }
    IEnumerable<IMultiFunctionNode> MultiFunctions { get; }
    IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
}

internal abstract class RangeNode : IRangeNode
{
    protected readonly IUserDefinedElements UserDefinedElements;
    protected readonly ICheckTypeProperties CheckTypeProperties;
    protected readonly IReferenceGenerator ReferenceGenerator;
    protected readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> CreateFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IMultiFunctionNode> _multiFunctionNodeFactory;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodeFactory;
    protected readonly Dictionary<ITypeSymbol, List<ICreateFunctionNode>> _createFunctions = new(CustomSymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<ITypeSymbol, List<IMultiFunctionNode>> _multiFunctions = new(CustomSymbolEqualityComparer.IncludeNullability);

    private readonly Dictionary<ITypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodes = new(CustomSymbolEqualityComparer.IncludeNullability);

    public abstract string FullName { get; }
    public string Name { get; }
    public abstract DisposalType DisposalType { get; }
    public IDisposalHandlingNode DisposalHandling { get; }
    public bool AddForDisposal { get; }
    public bool AddForDisposalAsync { get; }
    public abstract string? ContainerReference { get; }

    public IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction, IOnAwait onAwait) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _multiFunctions,
            () => _multiFunctionNodeFactory(
                type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList(),
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateCall(null, callingFunction, onAwait));

    public IEnumerable<ICreateFunctionNode> CreateFunctions => _createFunctions.Values.SelectMany(l => l);

    public IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups =>
        _rangedInstanceFunctionGroupNodes.Values;

    public IEnumerable<IMultiFunctionNode> MultiFunctions => _multiFunctions.Values.SelectMany(l => l);

    internal RangeNode(
        string name,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IMultiFunctionNode> multiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory)
    {
        UserDefinedElements = userDefinedElements;
        CheckTypeProperties = checkTypeProperties;
        ReferenceGenerator = referenceGenerator;
        CreateFunctionNodeFactory = createFunctionNodeFactory;
        _multiFunctionNodeFactory = multiFunctionNodeFactory;
        _rangedInstanceFunctionGroupNodeFactory = rangedInstanceFunctionGroupNodeFactory;
        Name = name;

        DisposalHandling = disposalHandlingNodeFactory(referenceGenerator);

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
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList(),
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateCall(null, callingFunction, callingFunction));

    public ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ScopeManager.GetTransientScope(type).BuildTransientScopeCallFunction(ContainerParameterForScope, type, this, callingFunction);

    public IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ScopeManager.GetScope(type).BuildScopeCallFunction(ContainerParameterForScope, TransientScopeInterfaceParameterForScope, type, this, callingFunction);

    protected IFunctionCallNode BuildRangedInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction, ScopeLevel level)
    {
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(type, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                level,
                type,
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator)
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
                type,
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator)
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