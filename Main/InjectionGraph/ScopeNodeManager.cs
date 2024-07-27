using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ScopeNodeManager : IContainerInstance
{
    private readonly Dictionary<string, NonContainerScopeNode> _scopeNodes = [];
    private readonly Dictionary<NonContainerScopeNode, Dictionary<ScopeNodeContext, ScopeNodeContext>> _scopeNodeContexts = [];

    private readonly Func<string, INamedTypeSymbol?, ScopeNode> _scopeFactory;
    private readonly Func<string, INamedTypeSymbol?, TransientScopeNode> _transientScopeFactory;
    private readonly Func<string, INamedTypeSymbol?, ScopeInfo, ScopeNodeConfigContext> _scopeCheckTypePropertiesFactory;
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
        Func<string, INamedTypeSymbol?, ScopeInfo, ScopeNodeConfigContext> scopeCheckTypePropertiesFactory,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        ContainerScopeNode = containerScopeNodeFactory();
        _scopeFactory = scopeFactory;
        _transientScopeFactory = transientScopeFactory;
        _scopeCheckTypePropertiesFactory = scopeCheckTypePropertiesFactory;
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

        if (_scopeNodes.TryGetValue(scopeType.Name, out var scopeNode))
            return scopeNode;
        
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

        if (_scopeNodes.TryGetValue(transientScopeType.Name, out var scopeNode))
            return scopeNode;
        
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

    internal ScopeNodeContext GetScopeNodeContext(ScopeNodeContext previousScopeNodeContext, TypeNode typeNode, ScopeLevel scopeNodeLevel)
    {
        var scopeNode = scopeNodeLevel switch
        {
            ScopeLevel.Scope => GetScope(typeNode.Type),
            ScopeLevel.TransientScope => GetTransientScope(typeNode.Type),
            _ => throw new ArgumentOutOfRangeException(new Guid("07D1B558-E50E-4C4F-9DF4-6B96700E911B").ToString())
        };
        scopeNode.AddScopeRoot(typeNode);
        if (_scopeNodeContexts.TryGetValue(scopeNode, out var contexts))
        {
            if (contexts.TryGetValue(previousScopeNodeContext, out var contextA))
                return contextA;
            contextA = CreateNewContext();
            contexts[previousScopeNodeContext] = contextA;
            return contextA;
        }

        contexts = [];
        var contextB = CreateNewContext();
        contexts[previousScopeNodeContext] = contextB;
        _scopeNodeContexts[scopeNode] = contexts;
        return contextB;

        ScopeNodeContext CreateNewContext()
        {
            var oldTransientScopeName = previousScopeNodeContext is ScopeNodeContext.TransientScope(var name) ? name : null;
            var scopeInfo = new ScopeInfo(scopeNode.Name, scopeNode.Type);
            var scopeNodeConfigContext = _scopeCheckTypePropertiesFactory(scopeNode.Name, scopeNode.Type, scopeInfo);
            return scopeNodeLevel switch
            {
                ScopeLevel.Scope => new ScopeNodeContext.Scope(scopeNode.Name, oldTransientScopeName)
                {
                    CheckTypeProperties = scopeNodeConfigContext.CheckTypeProperties, 
                    UserDefinedElements = scopeNodeConfigContext.UserDefinedElements
                },
                ScopeLevel.TransientScope => new ScopeNodeContext.TransientScope(scopeNode.Name)
                {
                    CheckTypeProperties = scopeNodeConfigContext.CheckTypeProperties, 
                    UserDefinedElements = scopeNodeConfigContext.UserDefinedElements
                },
                _ => throw new ArgumentOutOfRangeException(new Guid("56CF73C9-AE92-4B6C-BB98-90713F5C817F").ToString())
            };
        }
    }

    internal bool IsScopeType(INamedTypeSymbol scopeType) =>
        _scopeRootTypeToScopeType.Values.Any(t => CustomSymbolEqualityComparer.Default.Equals(t, scopeType))
        || _transientScopeRootTypeToScopeType.Values.Any(t => CustomSymbolEqualityComparer.Default.Equals(t, scopeType))
        || CustomSymbolEqualityComparer.Default.Equals(_defaultScope.Value.Type, scopeType)
        || CustomSymbolEqualityComparer.Default.Equals(_defaultTransientScope.Value.Type, scopeType);
}