using System.Threading;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ScopeNodeManager : IContainerInstance
{
    private readonly Dictionary<string, ScopeNodeBase> _scopeNodes = [];

    private readonly Func<string, INamedTypeSymbol?, ScopeNode> _scopeFactory;
    private readonly Func<string, INamedTypeSymbol?, TransientScopeNode> _transientScopeFactory;
    private readonly Lazy<ScopeNode> _defaultScope;
    private readonly Lazy<TransientScopeNode> _defaultTransientScope;
    private readonly Dictionary<ITypeSymbol, ScopeNode> _customScopes;
    private readonly Dictionary<ITypeSymbol, TransientScopeNode> _customTransientScopes;
    private readonly Dictionary<ITypeSymbol, INamedTypeSymbol> _transientScopeRootTypeToScopeType;
    private readonly Dictionary<ITypeSymbol, INamedTypeSymbol> _scopeRootTypeToScopeType;

    public ScopeNodeManager(
        ContainerInfo containerInfo,
        Func<ContainerScopeNode> containerScopeNodeFactory,
        Func<string, INamedTypeSymbol?, ScopeNode> scopeFactory,
        Func<string, INamedTypeSymbol?, TransientScopeNode> transientScopeFactory,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        ContainerScopeNode = containerScopeNodeFactory();
        _scopeFactory = scopeFactory;
        _transientScopeFactory = transientScopeFactory;
        _defaultScope = new Lazy<ScopeNode>(
            () =>
            {
                var defaultScopeType = containerInfo.ContainerType.GetTypeMembers(Constants.DefaultScopeName).FirstOrDefault();
                var node = scopeFactory(Constants.DefaultScopeName, defaultScopeType);
                _scopeNodes[Constants.DefaultScopeName] = node;
                return node;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _defaultTransientScope = new Lazy<TransientScopeNode>(
            () =>
            {
                var defaultTransientScopeType = containerInfo.ContainerType.GetTypeMembers(Constants.DefaultTransientScopeName).FirstOrDefault();
                var node = transientScopeFactory(Constants.DefaultTransientScopeName, defaultTransientScopeType);
                _scopeNodes[Constants.DefaultTransientScopeName] = node;
                return node;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
        _customScopes = new Dictionary<ITypeSymbol, ScopeNode>(CustomSymbolEqualityComparer.Default);
        _customTransientScopes = new Dictionary<ITypeSymbol, TransientScopeNode>(CustomSymbolEqualityComparer.Default);

        _transientScopeRootTypeToScopeType = containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomTransientScopeName, StringComparison.Ordinal))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
                .SelectMany(ad => ad
                    .ConstructorArguments
                    .SelectMany(tc => tc.Kind switch
                    {
                        TypedConstantKind.Type => [tc.Value as INamedTypeSymbol],
                        TypedConstantKind.Array => tc
                            .Values 
                            .Where(subTc => subTc.Kind == TypedConstantKind.Type)
                            .Select(subTc => subTc.Value as INamedTypeSymbol),
                        _ => []
                    }))
                .OfType<ITypeSymbol>()
                .Select(rootType => (rootType, nts)))
            .ToDictionary<(ITypeSymbol rootType, INamedTypeSymbol nts), ITypeSymbol, INamedTypeSymbol>(
                t => t.rootType, 
                t => t.nts,
                CustomSymbolEqualityComparer.Default);
        
        _scopeRootTypeToScopeType = containerInfo
            .ContainerType
            .GetTypeMembers()
            .Where(nts => nts.Name.StartsWith(Constants.CustomScopeName, StringComparison.Ordinal))
            .SelectMany(nts => nts.GetAttributes()
                .Where(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass,
                        wellKnownTypesMiscellaneous.CustomScopeForRootTypesAttribute))
                .SelectMany(ad => ad
                    .ConstructorArguments
                    .SelectMany(tc => tc.Kind switch
                    {
                        TypedConstantKind.Type => [tc.Value as INamedTypeSymbol],
                        TypedConstantKind.Array => tc
                            .Values 
                            .Where(subTc => subTc.Kind == TypedConstantKind.Type)
                            .Select(subTc => subTc.Value as INamedTypeSymbol),
                        _ => []
                    }))
                .OfType<ITypeSymbol>()
                .Select(rootType => (rootType, nts)))
            .ToDictionary<(ITypeSymbol rootType, INamedTypeSymbol nts), ITypeSymbol, INamedTypeSymbol>(
                t => t.rootType, 
                t => t.nts,
                CustomSymbolEqualityComparer.Default);
        
        
    }

    public NonContainerScopeNode GetScope(ITypeSymbol scopeRootType)
    {
        if (_customScopes.TryGetValue(scopeRootType, out var scope))
            return scope;
        
        if (!_scopeRootTypeToScopeType.TryGetValue(scopeRootType, out var scopeType)) 
            return _defaultScope.Value;
        
        var ret = _scopeFactory(scopeType.Name, scopeType);
        _customScopes[scopeRootType] = ret;
        _scopeNodes[scopeType.Name] = ret;
        return ret;
    }

    public NonContainerScopeNode GetTransientScope(ITypeSymbol transientScopeRootType)
    {
        if (_customTransientScopes.TryGetValue(transientScopeRootType, out var builder))
            return builder;

        if (!_transientScopeRootTypeToScopeType.TryGetValue(transientScopeRootType, out var transientScopeType)) 
            return _defaultTransientScope.Value;
        
        var ret = _transientScopeFactory(transientScopeType.Name, transientScopeType);
        _customTransientScopes[transientScopeRootType] = ret;
        _scopeNodes[transientScopeType.Name] = ret;
        return ret;
    }

    public IEnumerable<ScopeNode> Scopes
    {
        get
        {
            IEnumerable<ScopeNode> ret = _customScopes.Values;
            if (_defaultScope.IsValueCreated)
                ret = ret.Prepend(_defaultScope.Value);
            return ret;
        }
    }

    public IEnumerable<TransientScopeNode> TransientScopes
    {
        get
        {
            IEnumerable<TransientScopeNode> ret = _customTransientScopes.Values;
            if (_defaultTransientScope.IsValueCreated)
                ret = ret.Prepend(_defaultTransientScope.Value);
            return ret;
        }
    }

    internal ScopeNodeBase ContainerScopeNode { get; }
    internal ScopeNodeBase this[string name] => _scopeNodes[name];

    internal void RegisterContainerInstance(TypeNode node) =>
        ContainerScopeNode.AddScopedInstance(node);

    internal void RegisterScopedInstance(string scopeName, TypeNode node) => 
        _scopeNodes[scopeName].AddScopedInstance(node);
}