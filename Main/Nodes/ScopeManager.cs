using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes;

internal interface IScopeManager
{
    IScopeNode GetScope(INamedTypeSymbol scopeRootType);
    ITransientScopeNode GetTransientScope(INamedTypeSymbol transientScopeRootType);
    IEnumerable<IScopeNode> Scopes { get; }
    IEnumerable<ITransientScopeNode> TransientScopes { get; }
}

internal class ScopeManager : IScopeManager, IContainerInstance
{
    private readonly IContainerInfo _containerInfo;
    private readonly IContainerNode _container;
    private readonly ITransientScopeInterfaceNode _transientScopeInterface;
    private readonly ImmutableList<ITypesFromAttributesBase> _containerTypesFromAttributesList;
    private readonly IReferenceGenerator _referenceGenerator;

    private readonly Func<
        string,
        IContainerNode,
        ITransientScopeInterfaceNode,
        IScopeManager,
        IUserDefinedElements,
        ICheckTypeProperties,
        IReferenceGenerator,
        IScopeNode> _scopeFactory;
    private readonly Func<
        string,
        IContainerNode,
        IScopeManager,
        IUserDefinedElements,
        ICheckTypeProperties,
        IReferenceGenerator,
        ITransientScopeNode> _transientScopeFactory;
    private readonly Func<INamedTypeSymbol?, ImmutableArray<AttributeData>, ScopeTypesFromAttributesBase> _scopeTypesFromAttributesFactory;
    private readonly Func<IReadOnlyList<ITypesFromAttributesBase>, ICheckTypeProperties> _checkTypePropertiesFactory;
    private readonly Func<INamedTypeSymbol, INamedTypeSymbol, IUserDefinedElements> _userProvidedScopeElementsFactory;
    private readonly Lazy<IScopeNode> _defaultScope;
    private readonly Lazy<ITransientScopeNode> _defaultTransientScope;
    private readonly IDictionary<INamedTypeSymbol, IScopeNode> _customScopes;
    private readonly IDictionary<INamedTypeSymbol, ITransientScopeNode> _customTransientScopes;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> _transientScopeRootTypeToScopeType;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> _scopeRootTypeToScopeType;

    public ScopeManager(
        IContainerInfo containerInfo,
        IContainerNode container,
        ITransientScopeInterfaceNode transientScopeInterface,
        ImmutableList<ITypesFromAttributesBase> containerTypesFromAttributesList,
        IReferenceGenerator referenceGenerator,
        Func<
            string,
            IContainerNode,
            ITransientScopeInterfaceNode,
            IScopeManager,
            IUserDefinedElements,
            ICheckTypeProperties,
            IReferenceGenerator,
            IScopeNode> scopeFactory,
        Func<
            string,
            IContainerNode,
            IScopeManager,
            IUserDefinedElements,
            ICheckTypeProperties,
            IReferenceGenerator,
            ITransientScopeNode> transientScopeFactory,
        Func<INamedTypeSymbol?, ImmutableArray<AttributeData>, ScopeTypesFromAttributesBase> scopeTypesFromAttributesFactory,
        Func<IReadOnlyList<ITypesFromAttributesBase>, ICheckTypeProperties> checkTypePropertiesFactory,
        Func<INamedTypeSymbol, INamedTypeSymbol, IUserDefinedElements> userProvidedScopeElementsFactory,
        IUserDefinedElements emptyUserDefinedElements,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        _containerInfo = containerInfo;
        _container = container;
        _transientScopeInterface = transientScopeInterface;
        _containerTypesFromAttributesList = containerTypesFromAttributesList;
        _referenceGenerator = referenceGenerator;
        _scopeFactory = scopeFactory;
        _transientScopeFactory = transientScopeFactory;
        _scopeTypesFromAttributesFactory = scopeTypesFromAttributesFactory;
        _checkTypePropertiesFactory = checkTypePropertiesFactory;
        _userProvidedScopeElementsFactory = userProvidedScopeElementsFactory;
        _defaultScope = new Lazy<IScopeNode>(
            () =>
            {
                var defaultScopeType = containerInfo.ContainerType.GetTypeMembers(Constants.DefaultScopeName).FirstOrDefault();
                var defaultScopeTypesFromAttributes = scopeTypesFromAttributesFactory(
                    defaultScopeType, 
                    defaultScopeType?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty);
                return scopeFactory(
                    Constants.DefaultScopeName,
                    container,
                    _transientScopeInterface,
                    this,
                    defaultScopeType is {} 
                        ? userProvidedScopeElementsFactory(defaultScopeType, containerInfo.ContainerType) 
                        : emptyUserDefinedElements,
                    checkTypePropertiesFactory(containerTypesFromAttributesList.Add(defaultScopeTypesFromAttributes)),
                    referenceGenerator)
                    .EnqueueBuildJobTo(container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _defaultTransientScope = new Lazy<ITransientScopeNode>(
            () =>
            {
                var defaultTransientScopeType = containerInfo.ContainerType.GetTypeMembers(Constants.DefaultTransientScopeName).FirstOrDefault();
                var defaultTransientScopeTypesFromAttributes = scopeTypesFromAttributesFactory(
                    defaultTransientScopeType, 
                    defaultTransientScopeType?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty);
                var ret = transientScopeFactory(
                    Constants.DefaultTransientScopeName,
                    container, 
                    this,
                    defaultTransientScopeType is {} 
                        ? userProvidedScopeElementsFactory(defaultTransientScopeType, containerInfo.ContainerType) 
                        : emptyUserDefinedElements,
                    checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(defaultTransientScopeTypesFromAttributes)),
                    referenceGenerator)
                    .EnqueueBuildJobTo(container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
                _transientScopeInterface.RegisterRange(ret);
                return ret;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _customScopes = new Dictionary<INamedTypeSymbol, IScopeNode>(CustomSymbolEqualityComparer.Default);
        _customTransientScopes = new Dictionary<INamedTypeSymbol, ITransientScopeNode>(CustomSymbolEqualityComparer.Default);

        _transientScopeRootTypeToScopeType = containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
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
            .ToDictionary<(INamedTypeSymbol rootType, INamedTypeSymbol nts), INamedTypeSymbol, INamedTypeSymbol>(
                t => t.rootType, 
                t => t.nts,
                CustomSymbolEqualityComparer.Default);
        
        _scopeRootTypeToScopeType = containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
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
            .ToDictionary<(INamedTypeSymbol rootType, INamedTypeSymbol nts), INamedTypeSymbol, INamedTypeSymbol>(
                t => t.rootType, 
                t => t.nts,
                CustomSymbolEqualityComparer.Default);
    }

    public IScopeNode GetScope(INamedTypeSymbol scopeRootType)
    {
        if (_customScopes.TryGetValue(scopeRootType, out var scope))
            return scope;
        
        if (!_scopeRootTypeToScopeType.TryGetValue(scopeRootType, out var scopeType)) 
            return _defaultScope.Value;
        
        var scopeTypesFromAttributes = _scopeTypesFromAttributesFactory(scopeType, scopeType.GetAttributes());
        var ret = _scopeFactory(
            scopeType.Name,
            _container,
            _transientScopeInterface,
            this,
            _userProvidedScopeElementsFactory(scopeType, _containerInfo.ContainerType),
            _checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(scopeTypesFromAttributes)),
            _referenceGenerator)
            .EnqueueBuildJobTo(_container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        _customScopes[scopeRootType] = ret;
        return ret;
    }

    public ITransientScopeNode GetTransientScope(INamedTypeSymbol transientScopeRootType)
    {
        if (_customTransientScopes.TryGetValue(transientScopeRootType, out var builder))
            return builder;

        if (!_transientScopeRootTypeToScopeType.TryGetValue(transientScopeRootType, out var transientScopeType)) 
            return _defaultTransientScope.Value;
        
        var scopeTypesFromAttributes = _scopeTypesFromAttributesFactory(transientScopeType, transientScopeType.GetAttributes());
        var ret = _transientScopeFactory(
            transientScopeType.Name,
            _container,
            this,
            _userProvidedScopeElementsFactory(transientScopeType, _containerInfo.ContainerType),
            _checkTypePropertiesFactory(_containerTypesFromAttributesList.Add(scopeTypesFromAttributes)),
            _referenceGenerator)
            .EnqueueBuildJobTo(_container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
        _customTransientScopes[transientScopeRootType] = ret;
         _transientScopeInterface.RegisterRange(ret);
        return ret;
    }

    public IEnumerable<IScopeNode> Scopes
    {
        get
        {
            IEnumerable<IScopeNode> ret = _customScopes.Values;
            if (_defaultScope.IsValueCreated)
                ret = ret.Prepend(_defaultScope.Value);
            return ret;
        }
    }

    public IEnumerable<ITransientScopeNode> TransientScopes
    {
        get
        {
            IEnumerable<ITransientScopeNode> ret = _customTransientScopes.Values;
            if (_defaultTransientScope.IsValueCreated)
                ret = ret.Prepend(_defaultTransientScope.Value);
            return ret;
        }
    }
}