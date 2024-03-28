using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
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
    IDisposalHandlingNode DisposalHandling { get; }
    IMethodSymbol? AddForDisposal { get; }
    IMethodSymbol? AddForDisposalAsync { get; }
    string? ContainerReference { get; }
    IEnumerable<IInitializedInstanceNode> InitializedInstances { get; }
    string ResolutionCounterReference { get; }

    IFunctionCallNode BuildCreateCall(ITypeSymbol type, IFunctionNode callingFunction);
    IWrappedAsyncFunctionCallNode BuildAsyncCreateCall(
        MapperData mapperData, 
        ITypeSymbol type,
        SynchronicityDecision synchronicity, 
        IFunctionNode callingFunction);
    ITransientScopeCallNode BuildTransientScopeCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction,
        IElementNodeMapperBase transientScopeImplementationMapper);
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
    IScopeCallNode BuildScopeCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction,
        IElementNodeMapperBase scopeImplementationMapper);
    IInitializedInstanceNode? GetInitializedNode(INamedTypeSymbol type);
    IFunctionCallNode BuildInitializationCall(IFunctionNode callingFunction);

    void CycleDetectionAndReorderingOfInitializedInstances();

    string DisposeChunkMethodName { get; }
    string DisposeChunkAsyncMethodName { get; }
    string DisposeExceptionHandlingMethodName { get; }
    string DisposeExceptionHandlingAsyncMethodName { get; }
    void RegisterTypeForDisposal(INamedTypeSymbol type);
    IReadOnlyDictionary<DisposalType, IReadOnlyList<INamedTypeSymbol>> GetDisposalTypeToTypeFullNames();
    INodeGenerator GetGenerator();
    bool GenerateEmptyConstructor { get; }
}

internal abstract class RangeNode : IRangeNode
{
    private readonly IMapperDataToFunctionKeyTypeConverter _mapperDataToFunctionKeyTypeConverter;
    protected readonly ITypeParameterUtility TypeParameterUtility;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<MapperData, ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> _createFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> _multiFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueFunctionNodeRoot> _multiKeyValueFunctionNodeFactory;
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiKeyValueMultiFunctionNodeRoot> _multiKeyValueMultiFunctionNodeFactory;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodeFactory;
    protected readonly Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IVoidFunctionNodeRoot> VoidFunctionNodeFactory;
    // ReSharper disable once InconsistentNaming
    protected readonly Dictionary<ITypeSymbol, List<ICreateFunctionNodeBase>> _createFunctions = new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<ITypeSymbol, List<IMultiFunctionNodeBase>> _multiFunctions = new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);
    private readonly Dictionary<ITypeSymbol, Dictionary<object, Dictionary<ITypeSymbol, List<IMultiFunctionNodeBase>>>> _keyedMultiFunctions = 
        new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);

    private readonly HashSet<INamedTypeSymbol> _aggregatedTypesForDisposal = 
        new(CustomSymbolEqualityComparer.IncludeNullability);

    private readonly Dictionary<ITypeSymbol, IRangedInstanceFunctionGroupNode> _rangedInstanceFunctionGroupNodes = new(MatchTypeParametersSymbolEqualityComparer.IncludeNullability);

    // ReSharper disable once InconsistentNaming
    protected readonly List<IVoidFunctionNode> _initializationFunctions = new();

    protected readonly Dictionary<INamedTypeSymbol, IInitializedInstanceNode> InitializedInstanceNodesMap = new(CustomSymbolEqualityComparer.IncludeNullability);
    private readonly INamedTypeSymbol _objectType;

    public abstract string FullName { get; }
    public string Name { get; }
    public IDisposalHandlingNode DisposalHandling { get; }
    public IMethodSymbol? AddForDisposal { get; }
    public IMethodSymbol? AddForDisposalAsync { get; }
    public abstract string? ContainerReference { get; }

    public IEnumerable<IInitializedInstanceNode> InitializedInstances => InitializedInstanceNodesMap.Values;
    public string ResolutionCounterReference { get; }

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
        IRangeUtility rangeUtility,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
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
        _checkTypeProperties = checkTypeProperties;
        _referenceGenerator = referenceGenerator;
        _createFunctionNodeFactory = createFunctionNodeFactory;
        _multiFunctionNodeFactory = multiFunctionNodeFactory;
        _multiKeyValueFunctionNodeFactory = multiKeyValueFunctionNodeFactory;
        _multiKeyValueMultiFunctionNodeFactory = multiKeyValueMultiFunctionNodeFactory;
        _rangedInstanceFunctionGroupNodeFactory = rangedInstanceFunctionGroupNodeFactory;
        VoidFunctionNodeFactory = voidFunctionNodeFactory;
        Name = name;

        DisposalHandling = disposalHandlingNodeFactory();

        if (userDefinedElements.AddForDisposal is not null)
        {
            AddForDisposal = userDefinedElements.AddForDisposal;
            DisposalHandling.RegisterSyncDisposal();
        }

        if (userDefinedElements.AddForDisposalAsync is not null)
        {
            AddForDisposalAsync = userDefinedElements.AddForDisposalAsync;
            DisposalHandling.RegisterAsyncDisposal();
        }

        _objectType = wellKnownTypes.Object;
        
        if (rangeType is not null)
        {
            var types = rangeUtility.GetRangeAttributes(rangeType)
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        wellKnownTypesMiscellaneous.InitializedInstancesAttribute))
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
        
        ResolutionCounterReference = referenceGenerator.Generate("resolutionCounter");
        DisposeChunkMethodName = referenceGenerator.Generate("DisposeChunk");
        DisposeChunkAsyncMethodName = referenceGenerator.Generate("DisposeChunkAsync");
        DisposeExceptionHandlingMethodName = referenceGenerator.Generate("DisposeExceptionHandling");
        DisposeExceptionHandlingAsyncMethodName = referenceGenerator.Generate("DisposeExceptionHandlingAsync");
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

    public string DisposeChunkMethodName { get; }
    public string DisposeChunkAsyncMethodName { get; }
    public string DisposeExceptionHandlingMethodName { get; }
    public string DisposeExceptionHandlingAsyncMethodName { get; }

    public void RegisterTypeForDisposal(INamedTypeSymbol type) => 
        _aggregatedTypesForDisposal.Add(type.UnboundIfGeneric());

    public IReadOnlyDictionary<DisposalType, IReadOnlyList<INamedTypeSymbol>> GetDisposalTypeToTypeFullNames() =>
        _aggregatedTypesForDisposal
            .SelectMany<INamedTypeSymbol, (DisposalType, INamedTypeSymbol)>(t =>
            {
                var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(t);
                return disposalType switch
                {
                    DisposalType.Async | DisposalType.Sync => [(DisposalType.Async, t), (DisposalType.Sync, t)],
                    DisposalType.Async => [(DisposalType.Async, t)],
                    DisposalType.Sync => [(DisposalType.Sync, t)],
                    _ => []
                };
            })
            .GroupBy(t => t.Item1)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<INamedTypeSymbol>) g.Select(t => t.Item2).ToList());

    public abstract INodeGenerator GetGenerator();
    public abstract bool GenerateEmptyConstructor { get; }

    public ITransientScopeCallNode BuildTransientScopeCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction,
        IElementNodeMapperBase transientScopeImplementationMapper) => 
        ScopeManager.GetTransientScope(type).BuildTransientScopeCallFunction(ContainerParameterForScope, type, this, callingFunction, transientScopeImplementationMapper);

    public IScopeCallNode BuildScopeCall(
        INamedTypeSymbol type, 
        IFunctionNode callingFunction,
        IElementNodeMapperBase scopeImplementationMapper) => 
        ScopeManager.GetScope(type)
            .BuildScopeCallFunction(ContainerParameterForScope,
                TransientScopeInterfaceParameterForScope,
                type,
                this,
                callingFunction,
                scopeImplementationMapper);

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