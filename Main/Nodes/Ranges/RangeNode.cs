using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IRangeNode : INode
{
    string FullName { get; }
    string Name { get; }
    DisposalType DisposalType { get; }
    IDisposalHandlingNode DisposalHandling { get; }
    IMethodSymbol? AddForDisposal { get; }
    IMethodSymbol? AddForDisposalAsync { get; }
    string? ContainerReference { get; }
    IEnumerable<IInitializedInstanceNode> InitializedInstances { get; }

    IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction);
    IWrappedAsyncFunctionCallNode BuildAsyncCreateCall(
        MapperData mapperData, 
        ITypeSymbol type,
        SynchronicityDecision synchronicity, 
        IFunctionNode callingFunction);
    ITransientScopeCallNode BuildTransientScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IRangedInstanceFunctionNode BuildTransientScopeFunction(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction, PassedContext passedContext);
    IFunctionCallNode BuildEnumerableKeyValueCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IFunctionCallNode BuildEnumerableKeyValueMultiCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IEnumerable<ICreateFunctionNodeBase> CreateFunctions { get; }
    IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups { get; }
    bool HasGenericRangeInstanceFunctionGroups { get; }
    string RangedInstanceStorageFieldName { get; }
    void AdjustRangedInstancesIfGeneric();
    IEnumerable<IMultiFunctionNodeBase> MultiFunctions { get; }
    IEnumerable<IVoidFunctionNode> InitializationFunctions { get; }
    IScopeCallNode BuildScopeCall(INamedTypeSymbol type, IFunctionNode callingFunction);
    IInitializedInstanceNode? GetInitializedNode(INamedTypeSymbol type);
    IFunctionCallNode BuildInitializationCall(IFunctionNode callingFunction);

    void CycleDetectionAndReorderingOfInitializedInstances();
}

internal abstract class RangeNode : IRangeNode
{
    private readonly IMapperDataToFunctionKeyTypeConverter _mapperDataToFunctionKeyTypeConverter;
    protected readonly ITypeParameterUtility TypeParameterUtility;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> _createFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> _multiFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueFunctionNodeRoot> _multiKeyValueFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueMultiFunctionNodeRoot> _multiKeyValueMultiFunctionNodeFactory;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodeFactory;
    protected readonly Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> VoidFunctionNodeFactory;
    protected readonly Dictionary<ITypeSymbol, List<ICreateFunctionNodeBase>> _createFunctions = new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<ITypeSymbol, List<IMultiFunctionNodeBase>> _multiFunctions = new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<ITypeSymbol, Dictionary<object, Dictionary<ITypeSymbol, List<IMultiFunctionNodeBase>>>> _keyedMultiFunctions = 
        new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);

    private readonly Dictionary<ITypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodes = new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);

    protected readonly List<IVoidFunctionNode> _initializationFunctions = new();

    protected readonly Dictionary<INamedTypeSymbol, IInitializedInstanceNode> InitializedInstanceNodesMap = new(CustomSymbolEqualityComparer.IncludeNullability);
    private readonly INamedTypeSymbol _objectType;

    public abstract string FullName { get; }
    public string Name { get; }
    public abstract DisposalType DisposalType { get; }
    public IDisposalHandlingNode DisposalHandling { get; }
    public IMethodSymbol? AddForDisposal { get; }
    public IMethodSymbol? AddForDisposalAsync { get; }
    public abstract string? ContainerReference { get; }

    public IEnumerable<IInitializedInstanceNode> InitializedInstances => InitializedInstanceNodesMap.Values;

    public IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction,
        PassedContext passedContext) =>
        passedContext.InjectionKeyModification is { } injectionKey
            ? BuildEnumerableCall(type, callingFunction, injectionKey)
            : BuildEnumerableCall(type, callingFunction);

    private IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction)
    {
        return FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _multiFunctions,
            () => _multiFunctionNodeFactory(
                    type,
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateCall(type, null, callingFunction, TypeParameterUtility.ExtractTypeParameters(type)));
    }

    private IFunctionCallNode BuildEnumerableCall(INamedTypeSymbol type, IFunctionNode callingFunction, InjectionKey injectionKey)
    {
        if (!_keyedMultiFunctions.TryGetValue(injectionKey.Type, out var keyedMultiFunctions))
        {
            keyedMultiFunctions = new Dictionary<object, Dictionary<ITypeSymbol, List<IMultiFunctionNodeBase>>>();
            _keyedMultiFunctions[injectionKey.Type] = keyedMultiFunctions;
        }
        if (!keyedMultiFunctions.TryGetValue(injectionKey.Value, out var multiFunctions))
        {
            multiFunctions = new Dictionary<ITypeSymbol, List<IMultiFunctionNodeBase>>();
            keyedMultiFunctions[injectionKey.Value] = multiFunctions;
        }
        return FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            multiFunctions,
            () => _multiFunctionNodeFactory(
                    type,
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, injectionKey)),
            f => f.CreateCall(type, null, callingFunction, TypeParameterUtility.ExtractTypeParameters(type)));
    }

    public IFunctionCallNode BuildEnumerableKeyValueCall(INamedTypeSymbol type, IFunctionNode callingFunction)
    {
        return FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _multiFunctions,
            () => _multiKeyValueFunctionNodeFactory(
                    type,
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateCall(type, null, callingFunction, TypeParameterUtility.ExtractTypeParameters(type)));
    }

    public IFunctionCallNode BuildEnumerableKeyValueMultiCall(INamedTypeSymbol type, IFunctionNode callingFunction)
    {
        return FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _multiFunctions,
            () => _multiKeyValueMultiFunctionNodeFactory(
                    type,
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateCall(type, null, callingFunction, TypeParameterUtility.ExtractTypeParameters(type)));
    }

    public IEnumerable<ICreateFunctionNodeBase> CreateFunctions => _createFunctions.Values.SelectMany(l => l);

    public IEnumerable<IRangedInstanceFunctionGroupNode> RangedInstanceFunctionGroups =>
        _rangedInstanceFunctionGroupNodes.Values;

    public bool HasGenericRangeInstanceFunctionGroups => _rangedInstanceFunctionGroupNodes
        .Values
        .Any(g => g.IsOpenGeneric);

    private string? _rangedInstanceStorageFieldName;
    public string RangedInstanceStorageFieldName => _rangedInstanceStorageFieldName ??= _referenceGenerator.Generate("rangedInstanceStorage");

    public void AdjustRangedInstancesIfGeneric()
    {
        var groupedByUnbound = _rangedInstanceFunctionGroupNodes.GroupBy(kvp => kvp.Key is INamedTypeSymbol namedTypeSymbol
            ? namedTypeSymbol.UnboundIfGeneric()
            : kvp.Key);

        foreach (var group in groupedByUnbound)
        {
            if (group.Any(kvp => kvp.Value.IsOpenGeneric))
            {
                foreach (var kvp in group)
                    kvp.Value.OverrideIsOpenGenericToTrue();
            }
        }
    }

    public IEnumerable<IMultiFunctionNodeBase> MultiFunctions => _multiFunctions
        .Values
        .SelectMany(l => l)
        .Concat(_keyedMultiFunctions
            .Values
            .SelectMany(d => d
                .Values
                .SelectMany(l => l)
                .SelectMany(l => l.Value)));
    public IEnumerable<IVoidFunctionNode> InitializationFunctions => _initializationFunctions;

    internal RangeNode(
        string name,
        INamedTypeSymbol? rangeType,
        IUserDefinedElements userDefinedElements,
        IMapperDataToFunctionKeyTypeConverter mapperDataToFunctionKeyTypeConverter,
        ITypeParameterUtility typeParameterUtility,
        IContainerWideContext containerWideContext,
        IReferenceGenerator referenceGenerator,
        Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueFunctionNodeRoot> multiKeyValueFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueMultiFunctionNodeRoot> multiKeyValueMultiFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory)
    {
        _mapperDataToFunctionKeyTypeConverter = mapperDataToFunctionKeyTypeConverter;
        TypeParameterUtility = typeParameterUtility;
        _referenceGenerator = referenceGenerator;
        _createFunctionNodeFactory = createFunctionNodeFactory;
        _multiFunctionNodeFactory = multiFunctionNodeFactory;
        _multiKeyValueFunctionNodeFactory = multiKeyValueFunctionNodeFactory;
        _multiKeyValueMultiFunctionNodeFactory = multiKeyValueMultiFunctionNodeFactory;
        _rangedInstanceFunctionGroupNodeFactory = rangedInstanceFunctionGroupNodeFactory;
        VoidFunctionNodeFactory = voidFunctionNodeFactory;
        Name = name;

        DisposalHandling = disposalHandlingNodeFactory();

        if (userDefinedElements.AddForDisposal is { })
        {
            AddForDisposal = userDefinedElements.AddForDisposal;
            DisposalHandling.RegisterSyncDisposal();
        }

        if (userDefinedElements.AddForDisposalAsync is { })
        {
            AddForDisposalAsync = userDefinedElements.AddForDisposalAsync;
            DisposalHandling.RegisterAsyncDisposal();
        }

        _objectType = containerWideContext.WellKnownTypes.Object;
        
        if (rangeType is { })
        {
            var types = rangeType
                .GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        containerWideContext.WellKnownTypesMiscellaneous.InitializedInstancesAttribute))
                .Where(ad =>
                    ad is { ConstructorArguments.Length: 1 } &&
                    ad.ConstructorArguments[0].Kind == TypedConstantKind.Array)
                .SelectMany(ad => ad
                    .ConstructorArguments[0]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>());
            foreach (var type in types)
                InitializedInstanceNodesMap[type] = initializedInstanceNodeFactory(type);
        }
    }
    
    protected abstract IScopeManager ScopeManager { get; }
    
    protected abstract IContainerNode ParentContainer { get; }
    
    protected abstract string ContainerParameterForScope { get; }

    protected virtual string TransientScopeInterfaceParameterForScope => Constants.ThisKeyword;

    public virtual void Build(PassedContext passedContext) {}

    public abstract void Accept(INodeVisitor nodeVisitor);

    public IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _createFunctions,
            () => _createFunctionNodeFactory(
                    new VanillaMapperData(),
                    TypeParameterUtility.ReplaceTypeParametersByCustom(type),
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateCall(type, null, callingFunction, TypeParameterUtility.ExtractTypeParameters(type)));

    public IWrappedAsyncFunctionCallNode BuildAsyncCreateCall(
        MapperData mapperData, 
        ITypeSymbol type, 
        SynchronicityDecision synchronicity,
        IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            _mapperDataToFunctionKeyTypeConverter.Convert(mapperData, type),
            callingFunction,
            _createFunctions,
            () => _createFunctionNodeFactory(
                    mapperData,
                    TypeParameterUtility.ReplaceTypeParametersByCustom(type),
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)),
            f => f.CreateAsyncCall(type, null, synchronicity, callingFunction, TypeParameterUtility.ExtractTypeParameters(type)));

    public IFunctionCallNode BuildInitializationCall(IFunctionNode callingFunction)
    {
        var voidFunction = FunctionResolutionUtility.GetOrCreateFunction(
            callingFunction,
            _initializationFunctions,
            () => VoidFunctionNodeFactory(
                    InitializedInstances.ToList(),
                    callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)));

        return voidFunction.CreateCall(_objectType, null, callingFunction, Array.Empty<ITypeSymbol>());
    }

    public void CycleDetectionAndReorderingOfInitializedInstances()
    {
        foreach (var initializationFunction in _initializationFunctions)
            initializationFunction.ReorderOrDetectCycle();
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
        var preparedType = TypeParameterUtility.ReplaceTypeParametersByCustom(type);
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(preparedType, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                level,
                type)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
            _rangedInstanceFunctionGroupNodes[preparedType] = rangedInstanceFunctionGroupNode;
        }
        var function = rangedInstanceFunctionGroupNode.BuildFunction(callingFunction);
        return function.CreateCall(type, ownerReference, callingFunction, TypeParameterUtility.ExtractTypeParameters(type));
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
        var unbound = type.UnboundIfGeneric();
        if (!_rangedInstanceFunctionGroupNodes.TryGetValue(unbound, out var rangedInstanceFunctionGroupNode))
        {
            rangedInstanceFunctionGroupNode = _rangedInstanceFunctionGroupNodeFactory(
                ScopeLevel.TransientScope,
                type)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
            _rangedInstanceFunctionGroupNodes[unbound] = rangedInstanceFunctionGroupNode;
        }
        var function = rangedInstanceFunctionGroupNode.BuildFunction(callingFunction);
        function.CreateCall(type, null, callingFunction, TypeParameterUtility.ExtractTypeParameters(type));
        return function;
    }

    public IFunctionCallNode BuildScopeInstanceCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction) =>
        BuildRangedInstanceCall(null, type, callingFunction, ScopeLevel.Scope);
}