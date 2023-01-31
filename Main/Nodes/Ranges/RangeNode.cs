using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IRangeNode : INode
{
    string FullName { get; }
    string Name { get; }
    DisposalType DisposalType { get; }
    IDisposalHandlingNode DisposalHandling { get; }
    bool AddForDisposal { get; }
    bool AddForDisposalAsync { get; }

    IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction);
    ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IRangedInstanceFunctionNode BuildTransientScopeFunction(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IReadOnlyList<ICreateFunctionNode> CreateFunctions { get; }
    IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups { get; }
    IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
}

internal abstract class RangeNode : IRangeNode
{
    protected readonly IUserDefinedElements UserDefinedElements;
    protected readonly ICheckTypeProperties CheckTypeProperties;
    protected readonly IReferenceGenerator ReferenceGenerator;
    protected readonly Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> CreateFunctionNodeFactory;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodeFactory;
    protected readonly List<ICreateFunctionNode> _createFunctions = new();

    private readonly Dictionary<TypeKey, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodes = new();

    public abstract string FullName { get; }
    public string Name { get; }
    public abstract DisposalType DisposalType { get; }
    public IDisposalHandlingNode DisposalHandling { get; }
    public bool AddForDisposal { get; }
    public bool AddForDisposalAsync { get; }

    public IReadOnlyList<ICreateFunctionNode> CreateFunctions => _createFunctions;

    public IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups =>
        _rangedInstanceFunctionGroupNodes.Values;

    internal RangeNode(
        string name,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> createFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory)
    {
        UserDefinedElements = userDefinedElements;
        CheckTypeProperties = checkTypeProperties;
        ReferenceGenerator = referenceGenerator;
        CreateFunctionNodeFactory = createFunctionNodeFactory;
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

    public virtual void Build() {}

    public abstract void Accept(INodeVisitor nodeVisitor);
    public IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction)
    {
        // todo smarter overloads handling
        var createFunction = CreateFunctionNodeFactory(
            type,
            callingFunction.Overrides.Select(kvp => kvp.Value.Item1).ToList(),
            this,
            ParentContainer,
            UserDefinedElements,
            CheckTypeProperties,
            ReferenceGenerator).EnqueueTo(ParentContainer.BuildQueue);
        _createFunctions.Add(createFunction);
        
        return createFunction.CreateCall(null, callingFunction);
    }

    public ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction)=> 
        ScopeManager.GetTransientScope(type).BuildTransientScopeCallFunction(ContainerParameterForScope, type, callingFunction);

    public IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ScopeManager.GetScope(type).BuildScopeCallFunction(ContainerParameterForScope, TransientScopeInterfaceParameterForScope, type, this, callingFunction);

    protected IFunctionCallNode BuildRangedInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction, ScopeLevel level)
    {
        var typeKey = type.ToTypeKey();
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(typeKey, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                level,
                type,
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator).EnqueueTo(ParentContainer.BuildQueue);
            _rangedInstanceFunctionGroupNodes[typeKey] = rangedInstanceFunctionGroupNode;
        }
        var function = rangedInstanceFunctionGroupNode.BuildFunction(callingFunction);
        return function.CreateCall(ownerReference, callingFunction);
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
        var typeKey = type.ToTypeKey();
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(typeKey, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                ScopeLevel.TransientScope,
                type,
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator).EnqueueTo(ParentContainer.BuildQueue);
            _rangedInstanceFunctionGroupNodes[typeKey] = rangedInstanceFunctionGroupNode;
        }
        var function = rangedInstanceFunctionGroupNode.BuildFunction(callingFunction);
        function.CreateCall(null, callingFunction);
        return function;
    }

    public IFunctionCallNode BuildScopeInstanceCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction) =>
        BuildRangedInstanceCall(null, type, callingFunction, ScopeLevel.Scope);
}