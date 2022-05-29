using System.Threading;
using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IScopeManager
{
    IScopeResolutionBuilder GetScopeBuilder(INamedTypeSymbol scopeRootType);
    ITransientScopeResolutionBuilder GetTransientScopeBuilder(INamedTypeSymbol transientScopeRootType);
    
    bool HasWorkToDo { get; }

    void DoWork();

    (IReadOnlyList<TransientScopeResolution>, IReadOnlyList<ScopeResolution>) Build();
}

internal class ScopeManager : IScopeManager
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly ImmutableList<ITypesFromAttributes> _containerTypesFromAttributesList;

    private readonly Func<
            string, 
            IContainerResolutionBuilder,
            ITransientScopeInterfaceResolutionBuilder ,
            IScopeManager,
            IUserProvidedScopeElements, 
            ICheckTypeProperties, 
            IScopeResolutionBuilder> _scopeResolutionBuilderFactory;
    private readonly Func<
        string, 
        IContainerResolutionBuilder,
        ITransientScopeInterfaceResolutionBuilder ,
        IScopeManager,
        IUserProvidedScopeElements, 
        ICheckTypeProperties, 
        ITransientScopeResolutionBuilder> _transientScopeResolutionBuilderFactory;
    private readonly Func<ImmutableArray<AttributeData>, ScopeTypesFromAttributes> _scopeTypesFromAttributesFactory;
    private readonly Func<IReadOnlyList<ITypesFromAttributes>, ICheckTypeProperties> _checkTypePropertiesFactory;
    private readonly Func<INamedTypeSymbol, IUserProvidedScopeElements> _userProvidedScopeElementsFactory;
    private readonly Lazy<IScopeResolutionBuilder> _defaultScopeBuilder;
    private readonly Lazy<ITransientScopeResolutionBuilder> _defaultTransientScopeBuilder;
    private readonly IDictionary<INamedTypeSymbol, IScopeResolutionBuilder> _customScopeBuilders;
    private readonly IDictionary<INamedTypeSymbol, ITransientScopeResolutionBuilder> _customTransientScopeBuilders;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> _transientScopeRootTypeToScopeType;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> _scopeRootTypeToScopeType;

    public ScopeManager(
        IContainerInfo containerInfo,
        IContainerResolutionBuilder containerResolutionBuilder,
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        ImmutableList<ITypesFromAttributes> containerTypesFromAttributesList,
        Func<
            string, 
            IContainerResolutionBuilder,
            ITransientScopeInterfaceResolutionBuilder ,
            IScopeManager,
            IUserProvidedScopeElements, 
            ICheckTypeProperties, 
            ITransientScopeResolutionBuilder> transientScopeResolutionBuilderFactory,
        Func<
            string, 
            IContainerResolutionBuilder,
            ITransientScopeInterfaceResolutionBuilder ,
            IScopeManager,
            IUserProvidedScopeElements, 
            ICheckTypeProperties, 
            IScopeResolutionBuilder> scopeResolutionBuilderFactory,
        Func<ImmutableArray<AttributeData>, ScopeTypesFromAttributes> scopeTypesFromAttributesFactory,
        Func<IReadOnlyList<ITypesFromAttributes>, ICheckTypeProperties> checkTypePropertiesFactory,
        Func<INamedTypeSymbol, IUserProvidedScopeElements> userProvidedScopeElementsFactory,
        IUserProvidedScopeElements emptyUserProvidedScopeElements,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _containerTypesFromAttributesList = containerTypesFromAttributesList;
        _scopeResolutionBuilderFactory = scopeResolutionBuilderFactory;
        _transientScopeResolutionBuilderFactory = transientScopeResolutionBuilderFactory;
        _scopeTypesFromAttributesFactory = scopeTypesFromAttributesFactory;
        _checkTypePropertiesFactory = checkTypePropertiesFactory;
        _userProvidedScopeElementsFactory = userProvidedScopeElementsFactory;
        _defaultScopeBuilder = new Lazy<IScopeResolutionBuilder>(
            () =>
            {
                var defaultScopeType = containerInfo.ContainerType.GetTypeMembers(Constants.DefaultScopeName).FirstOrDefault();
                var defaultScopeTypesFromAttributes = _scopeTypesFromAttributesFactory(defaultScopeType?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty);
                return _scopeResolutionBuilderFactory(
                    Constants.DefaultScopeName,
                    _containerResolutionBuilder,
                    _transientScopeInterfaceResolutionBuilder,
                    this,
                    defaultScopeType is {} 
                        ? _userProvidedScopeElementsFactory(defaultScopeType) 
                        : emptyUserProvidedScopeElements,
                    _checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(defaultScopeTypesFromAttributes)));
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _defaultTransientScopeBuilder = new Lazy<ITransientScopeResolutionBuilder>(
            () =>
            {
                var defaultTransientScopeType = containerInfo.ContainerType.GetTypeMembers(Constants.DefaultTransientScopeName).FirstOrDefault();
                var defaultTransientScopeTypesFromAttributes = _scopeTypesFromAttributesFactory(defaultTransientScopeType?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty);
                var ret = _transientScopeResolutionBuilderFactory(
                    Constants.DefaultTransientScopeName,
                    _containerResolutionBuilder, 
                    _transientScopeInterfaceResolutionBuilder, 
                    this,
                    defaultTransientScopeType is {} 
                        ? _userProvidedScopeElementsFactory(defaultTransientScopeType) 
                        : emptyUserProvidedScopeElements,
                    _checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(defaultTransientScopeTypesFromAttributes)));
                _transientScopeInterfaceResolutionBuilder.AddImplementation(ret);
                return ret;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _customScopeBuilders = new Dictionary<INamedTypeSymbol, IScopeResolutionBuilder>(SymbolEqualityComparer.Default);
        _customTransientScopeBuilders = new Dictionary<INamedTypeSymbol, ITransientScopeResolutionBuilder>(SymbolEqualityComparer.Default);

        _transientScopeRootTypeToScopeType = containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    SymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
                .SelectMany(ad => ad
                    .ConstructorArguments
                    .SelectMany(tc => tc.Kind switch
                    {
                        TypedConstantKind.Type => new [] { tc.Value as INamedTypeSymbol },
                        TypedConstantKind.Array => tc
                            .Values 
                            .Where(subTc => subTc.Kind == TypedConstantKind.Type)
                            .Select(subTc => subTc.Value as INamedTypeSymbol),
                        _ => Array.Empty<INamedTypeSymbol>()
                    }))
                .OfType<INamedTypeSymbol>()
                .Select(rootType => (rootType, nts)))
            .ToDictionary(t => t.rootType, t => t.nts);
        
        _scopeRootTypeToScopeType = containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    SymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
                .SelectMany(ad => ad
                    .ConstructorArguments
                    .SelectMany(tc => tc.Kind switch
                    {
                        TypedConstantKind.Type => new [] { tc.Value as INamedTypeSymbol },
                        TypedConstantKind.Array => tc
                            .Values 
                            .Where(subTc => subTc.Kind == TypedConstantKind.Type)
                            .Select(subTc => subTc.Value as INamedTypeSymbol),
                        _ => Array.Empty<INamedTypeSymbol>()
                    }))
                .OfType<INamedTypeSymbol>()
                .Select(rootType => (rootType, nts)))
            .ToDictionary(t => t.rootType, t => t.nts);
    }

    public IScopeResolutionBuilder GetScopeBuilder(INamedTypeSymbol scopeRootType)
    {
        if (_customScopeBuilders.TryGetValue(scopeRootType, out var builder))
            return builder;
        
        if (!_scopeRootTypeToScopeType.TryGetValue(scopeRootType, out var scopeType)) 
            return _defaultScopeBuilder.Value;
        
        var scopeTypesFromAttributes = _scopeTypesFromAttributesFactory(scopeType.GetAttributes());
        var ret = _scopeResolutionBuilderFactory(
            scopeType.Name,
            _containerResolutionBuilder,
            _transientScopeInterfaceResolutionBuilder,
            this,
            _userProvidedScopeElementsFactory(scopeType),
            _checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(scopeTypesFromAttributes)));
        _customScopeBuilders[scopeRootType] = ret;
        return ret;
    }

    public ITransientScopeResolutionBuilder GetTransientScopeBuilder(INamedTypeSymbol transientScopeRootType)
    {
        if (_customTransientScopeBuilders.TryGetValue(transientScopeRootType, out var builder))
            return builder;

        if (!_transientScopeRootTypeToScopeType.TryGetValue(transientScopeRootType, out var scopeType)) 
            return _defaultTransientScopeBuilder.Value;
        
        var scopeTypesFromAttributes = _scopeTypesFromAttributesFactory(scopeType.GetAttributes());
        var ret = _transientScopeResolutionBuilderFactory(
            scopeType.Name,
            _containerResolutionBuilder,
            _transientScopeInterfaceResolutionBuilder,
            this,
            _userProvidedScopeElementsFactory(scopeType),
            _checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(scopeTypesFromAttributes)));
        _customTransientScopeBuilders[transientScopeRootType] = ret;
        _transientScopeInterfaceResolutionBuilder.AddImplementation(ret);
        return ret;
    }

    public bool HasWorkToDo =>
        _defaultScopeBuilder.IsValueCreated && _defaultScopeBuilder.Value.HasWorkToDo
        || _defaultTransientScopeBuilder.IsValueCreated && _defaultTransientScopeBuilder.Value.HasWorkToDo
        || _customScopeBuilders.Values.Any(sb => sb.HasWorkToDo)
        || _customTransientScopeBuilders.Values.Any(tsb => tsb.HasWorkToDo);
    
    public void DoWork()
    {
        if (_defaultScopeBuilder.IsValueCreated) _defaultScopeBuilder.Value.DoWork();
        if (_defaultTransientScopeBuilder.IsValueCreated) _defaultTransientScopeBuilder.Value.DoWork();
        foreach (var customScopeBuilder in _customScopeBuilders.Values.ToList())
            customScopeBuilder.DoWork();
        foreach (var customTransientScopeBuilder in _customTransientScopeBuilders.Values.ToList())
            customTransientScopeBuilder.DoWork();
    }

    public (IReadOnlyList<TransientScopeResolution>, IReadOnlyList<ScopeResolution>) Build()
    {
        return (
            _customTransientScopeBuilders
                .Values
                .Prepend(_defaultTransientScopeBuilder.IsValueCreated ? _defaultTransientScopeBuilder.Value : null)
                .OfType<ITransientScopeResolutionBuilder>()
                .Select(tsb => tsb.Build())
                .ToList(),
            _customScopeBuilders
                .Values
                .Prepend(_defaultScopeBuilder.IsValueCreated ? _defaultScopeBuilder.Value : null)
                .OfType<IScopeResolutionBuilder>()
                .Select(sb => sb.Build())
                .ToList());
    }
}