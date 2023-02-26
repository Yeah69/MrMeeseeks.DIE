using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
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
    private readonly IContainerInfoContext _containerInfoContext;
    private readonly IContainerNode _container;
    private readonly IContainerTypesFromAttributes _containerTypesFromAttributes;
    private readonly ITransientScopeInterfaceNode _transientScopeInterface;
    private readonly IReferenceGenerator _referenceGenerator;

    private readonly Func<
        IScopeInfo,
        IContainerNode,
        ITransientScopeInterfaceNode,
        IScopeManager,
        IUserDefinedElementsBase,
        IScopeCheckTypeProperties,
        IReferenceGenerator,
        IScopeNodeRoot> _scopeFactory;
    private readonly Func<
        IScopeInfo,
        IContainerNode,
        IScopeManager,
        IUserDefinedElementsBase,
        IScopeCheckTypeProperties,
        IReferenceGenerator,
        ITransientScopeNodeRoot> _transientScopeFactory;
    private readonly Func<IScopeInfo, IContainerInfoContext, IScopeTypesFromAttributes> _scopeTypesFromAttributesFactory;
    private readonly Func<IScopeInfo, IContainerInfoContext, IContainerTypesFromAttributes, IScopeCheckTypeProperties> _checkTypePropertiesFactory;
    private readonly Func<(INamedTypeSymbol, INamedTypeSymbol), IUserDefinedElementsBase> _userProvidedScopeElementsFactory;
    private readonly Func<string, INamedTypeSymbol?, IScopeInfo> _scopeInfoFactory;
    private readonly Lazy<IScopeNode> _defaultScope;
    private readonly Lazy<ITransientScopeNode> _defaultTransientScope;
    private readonly IDictionary<INamedTypeSymbol, IScopeNode> _customScopes;
    private readonly IDictionary<INamedTypeSymbol, ITransientScopeNode> _customTransientScopes;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> _transientScopeRootTypeToScopeType;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> _scopeRootTypeToScopeType;

    public ScopeManager(
        IContainerInfoContext containerInfoContext,
        IContainerNode container,
        IContainerTypesFromAttributes containerTypesFromAttributes,
        ITransientScopeInterfaceNode transientScopeInterface,
        IReferenceGenerator referenceGenerator,
        Func<
            IScopeInfo,
            IContainerNode,
            ITransientScopeInterfaceNode,
            IScopeManager,
            IUserDefinedElementsBase,
            IScopeCheckTypeProperties,
            IReferenceGenerator,
            IScopeNodeRoot> scopeFactory,
        Func<
            IScopeInfo,
            IContainerNode,
            IScopeManager,
            IUserDefinedElementsBase,
            IScopeCheckTypeProperties,
            IReferenceGenerator,
            ITransientScopeNodeRoot> transientScopeFactory,
        Func<IScopeInfo, IContainerInfoContext, IScopeTypesFromAttributes> scopeTypesFromAttributesFactory,
        Func<IScopeInfo, IContainerInfoContext, IContainerTypesFromAttributes, IScopeCheckTypeProperties> checkTypePropertiesFactory,
        Func<(INamedTypeSymbol, INamedTypeSymbol), IUserDefinedElements> userProvidedScopeElementsFactory,
        Func<string, INamedTypeSymbol?, IScopeInfo> scopeInfoFactory,
        IEmptyUserDefinedElements emptyUserDefinedElementsBase,
        IContainerWideContext containerWideContext)
    {
        _containerInfo = containerInfoContext.ContainerInfo;
        _containerInfoContext = containerInfoContext;
        _container = container;
        _containerTypesFromAttributes = containerTypesFromAttributes;
        _transientScopeInterface = transientScopeInterface;
        _referenceGenerator = referenceGenerator;
        _scopeFactory = scopeFactory;
        _transientScopeFactory = transientScopeFactory;
        _scopeTypesFromAttributesFactory = scopeTypesFromAttributesFactory;
        _checkTypePropertiesFactory = checkTypePropertiesFactory;
        _userProvidedScopeElementsFactory = userProvidedScopeElementsFactory;
        _scopeInfoFactory = scopeInfoFactory;
        _defaultScope = new Lazy<IScopeNode>(
            () =>
            {
                var defaultScopeType = _containerInfo.ContainerType.GetTypeMembers(Constants.DefaultScopeName).FirstOrDefault();
                var scopeInfo = scopeInfoFactory(Constants.DefaultScopeName, defaultScopeType);
                return scopeFactory(
                        scopeInfo,
                        container,
                        _transientScopeInterface,
                        this,
                        defaultScopeType is {} 
                            ? userProvidedScopeElementsFactory((defaultScopeType, _containerInfo.ContainerType)) 
                            : emptyUserDefinedElementsBase,
                        checkTypePropertiesFactory(scopeInfo, containerInfoContext, containerTypesFromAttributes),
                        referenceGenerator)
                    .Scope
                    .EnqueueBuildJobTo(container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _defaultTransientScope = new Lazy<ITransientScopeNode>(
            () =>
            {
                var defaultTransientScopeType = _containerInfo.ContainerType.GetTypeMembers(Constants.DefaultTransientScopeName).FirstOrDefault();
                var scopeInfo = scopeInfoFactory(Constants.DefaultTransientScopeName, defaultTransientScopeType);
                var ret = transientScopeFactory(
                        scopeInfo,
                        container, 
                        this,
                        defaultTransientScopeType is {} 
                            ? userProvidedScopeElementsFactory((defaultTransientScopeType, _containerInfo.ContainerType)) 
                            : emptyUserDefinedElementsBase,
                        checkTypePropertiesFactory(scopeInfo, containerInfoContext, containerTypesFromAttributes),
                        referenceGenerator)
                    .TransientScope
                    .EnqueueBuildJobTo(container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
                _transientScopeInterface.RegisterRange(ret);
                return ret;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _customScopes = new Dictionary<INamedTypeSymbol, IScopeNode>(CustomSymbolEqualityComparer.Default);
        _customTransientScopes = new Dictionary<INamedTypeSymbol, ITransientScopeNode>(CustomSymbolEqualityComparer.Default);

        _transientScopeRootTypeToScopeType = _containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        containerWideContext.WellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
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
        
        _scopeRootTypeToScopeType = _containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        containerWideContext.WellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
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
        
        var scopeInfo = _scopeInfoFactory(scopeType.Name, scopeType);
        
        var ret = _scopeFactory(
                scopeInfo,
                _container,
                _transientScopeInterface,
                this,
                _userProvidedScopeElementsFactory((scopeType, _containerInfo.ContainerType)),
                _checkTypePropertiesFactory(scopeInfo, _containerInfoContext, _containerTypesFromAttributes),
                _referenceGenerator)
            .Scope
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
        
        var scopeInfo = _scopeInfoFactory(transientScopeType.Name, transientScopeType);
        
        var ret = _transientScopeFactory(
                scopeInfo,
                _container,
                this,
                _userProvidedScopeElementsFactory((transientScopeType, _containerInfo.ContainerType)),
                _checkTypePropertiesFactory(scopeInfo, _containerInfoContext, _containerTypesFromAttributes),
                _referenceGenerator)
            .TransientScope
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