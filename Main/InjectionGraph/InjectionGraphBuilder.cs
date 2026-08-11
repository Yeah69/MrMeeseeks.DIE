using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed record ResolutionStep(TypeNode Current, EdgeContext Context, Location CurrentResolvedLocation);

internal interface IInjectionGraphBuilder
{
    IReadOnlyList<ITypeNodeFunction> Functions { get; }
    void BuildForRootType(
        ITypeSymbol type, 
        string entryFunctionName, 
        IReadOnlyList<ITypeSymbol> overrides,
        Location createFunctionAttributeLocation);

    void SplitAsyncConcreteEdges();

    void AssignFunctions();
}

internal sealed class InjectionGraphBuilder(
    InjectionGraphBuilderResolutionSteps resolutionSteps,
    TypeNodeManager typeNodeManager,
    ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
    OverrideContextManager overrideContextManager,
    ScopeNodeContext.Container containerScopeNodeContext,
    ScopeNodeManager scopeNodeManager,
    ResolutionRegister resolutionRegister,
    EdgeRegistry edgeRegistry,
    TypeSymbolUtility typeSymbolUtility,
    AsyncAdjustments asyncAdjustments,
    Func<TypeNode, bool, TypeNodeFunction> functionFactory,
    Func<TypeNode, IConcreteNode, ConcreteAsyncEdge> concreteAsyncEdgeFactory,
    Func<(TypeNode, TypeNode), TypeTypeEdge> typeTypeEdgeFactory,
    WellKnownTypes wellKnownTypes,
    WellKnownTypesCollections wellKnownTypesCollections)
    : IInjectionGraphBuilder, IContainerInstance
{
    private readonly List<ITypeNodeFunction> _functions = [];
    private HashSet<int> _asyncResolutionIds = [];

    public IReadOnlyList<ITypeNodeFunction> Functions => _functions;

    public void BuildForRootType(
        ITypeSymbol rootType, 
        string entryFunctionName, 
        IReadOnlyList<ITypeSymbol> overrides,
        Location createFunctionAttributeLocation)
    {
        var overrideContext = overrideContextManager.GetOrAddContext(overrides);
        var rootEdgeContext = new EdgeContext(
            containerScopeNodeContext,
            overrideContext,
            new KeyContext.None1(),
            new CaseChoiceContext.None2(),
            ScopeRootFullNameForScope: null,
            resolutionRegister.GetNewResolutionId());
        var concreteEntryFunctionNodeData = new ConcreteEntryFunctionNodeData(entryFunctionName, rootType, overrides);
        var concreteEntryFunctionNode = concreteEntryFunctionNodeManager.GetOrAddNode(concreteEntryFunctionNodeData);
        var rootTypeNodes = concreteEntryFunctionNode.ConnectIfNotAlready(rootEdgeContext).Select(t => t.TypeNode).ToList();
        var queue = new Queue<ResolutionStep>();
        foreach (var rootTypeNode in rootTypeNodes)
            queue.Enqueue(new(rootTypeNode, rootEdgeContext, createFunctionAttributeLocation));
        var i = 0;
        while (queue.Count > 0 && i < 100)
        {
            i++;
            var (typeNode, edgeContext, currentResolvedLocation) = queue.Dequeue();
            MakeResolutionStep(typeNode, edgeContext, queue, currentResolvedLocation);
        }
    }

    public void SplitAsyncConcreteEdges()
    {
        var awaitedNodes = asyncAdjustments.AwaitedNodes;
        Adjust();

        var concreteSyncEdges = edgeRegistry.Edges.OfType<ConcreteSyncEdge>().ToImmutableArray();
        
        foreach (var concreteSyncEdge in concreteSyncEdges)
        {
            var groups = concreteSyncEdge.Contexts.GroupBy(c => _asyncResolutionIds.Contains(c.ResolutionId));
            var syncContextsGroup = ImmutableArray<EdgeContext>.Empty;
            var asyncContextsGroup = ImmutableArray<EdgeContext>.Empty;
            foreach (var group in groups)
            {
                if (group.Key)
                    asyncContextsGroup = [.. group];
                else
                    syncContextsGroup = [.. group];
            }

            if (asyncContextsGroup.Length > 0)
            {
                var concreteAsyncEdge = concreteAsyncEdgeFactory(concreteSyncEdge.Source, concreteSyncEdge.Target);
                foreach (var asyncContext in asyncContextsGroup)
                    concreteAsyncEdge.AddContext(asyncContext);
                concreteSyncEdge.Source.AddRegularOutgoing(concreteAsyncEdge);
            }

            if (syncContextsGroup.Length == 0)
            {
                concreteSyncEdge.Source.RemoveEdge(concreteSyncEdge);
                concreteSyncEdge.Target.RemoveEdge(concreteSyncEdge);
                edgeRegistry.Unregister(concreteSyncEdge);
            }
        }

        return;

        void Adjust()
        {
            EnhanceAwaitedNodes();
            foreach (var resolutionIds in awaitedNodes.Select(kvp => kvp.Value))
                _asyncResolutionIds.UnionWith(resolutionIds);
            return;

            void EnhanceAwaitedNodes()
            {
                var outerQueue = new Queue<(INode, int)>(awaitedNodes.SelectMany(kvp => kvp.Value.Select(ri => (kvp.Key, ri))));
                while (outerQueue.Count > 0)
                {
                    var (outerNode, resolutionId) = outerQueue.Dequeue();
                    var innerQueue = new Queue<INode>();
                    innerQueue.Enqueue(outerNode);
                    
                    var visitedNodes = new HashSet<INode>();
                    while (innerQueue.Count > 0)
                    {
                        var currentNode = innerQueue.Dequeue();
                        visitedNodes.Add(currentNode);
                        if (currentNode is IConcreteNode currentConcreteNode)
                        {
                            var sequence = currentConcreteNode
                                .IncomingEdges
                                .Where(e => e.Contexts.Any(c => c.ResolutionId == resolutionId))
                                .Select(e => e.SourceAsNode)
                                .Where(n => !visitedNodes.Contains(n));
                            foreach (var typeNode in sequence)
                                innerQueue.Enqueue(typeNode);
                        } 
                        else if (currentNode is TypeNode currentTypeNode)
                        {
                            var sequence = currentTypeNode
                                .IncomingEdges
                                .Where(e => e.Contexts.Any(c => c.ResolutionId == resolutionId))
                                .Select(e => e.SourceAsNode)
                                .Where(n => !visitedNodes.Contains(n));
                            foreach (var concreteNode in sequence)
                                innerQueue.Enqueue(concreteNode);

                            if (currentTypeNode.LinkedResolutionIdsToFrom.TryGetValue(resolutionId, out var linkedResolutionIds))
                                foreach (var linkedResolutionId in linkedResolutionIds)
                                    awaitedNodes.GetOrAdd(currentTypeNode, _ => []).Add(linkedResolutionId);
                        }
                    }
                }
            }
        }
    }

    private void MakeResolutionStep(
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        var typeNodeType = typeNode.Type;

        var scopeRootLevel = edgeContext.ScopeNode.CheckTypeProperties.ShouldBeScopeRoot(typeNode.Type);

        if (scopeRootLevel is ScopeLevel.Scope or ScopeLevel.TransientScope)
        {
            var scopeNodeContext = scopeNodeManager.GetScopeNodeContext(edgeContext.ScopeNode, typeNode, scopeRootLevel);
            var edgeContextForScopeRoot = edgeContext with { ScopeRootFullNameForScope = typeNode.Type.FullName() };
            typeNode.RegisterScopeRootConfiguration(scopeNodeContext, edgeContext.ScopeNode, CreateTypeTypeEdge);

            TypeTypeEdge? CreateTypeTypeEdge()
            {
                var scopeNode = scopeRootLevel is ScopeLevel.Scope 
                    ? scopeNodeManager.GetScope(typeNode.Type)
                    : scopeNodeManager.GetTransientScope(typeNode.Type);

                TypeTypeEdge? scopeRootTypeTypeEdge = null;
                if (scopeNode.Type is { } scopeType)
                {
                    var scopeRootTypeNode = typeNodeManager.GetOrAddNode(scopeType);
                    scopeRootTypeTypeEdge = typeTypeEdgeFactory((typeNode, scopeRootTypeNode));
                    scopeRootTypeTypeEdge.AddContext(edgeContextForScopeRoot);
                    queue.Enqueue(new (scopeRootTypeNode, edgeContextForScopeRoot, currentResolvedLocation));
                }
                return scopeRootTypeTypeEdge;
            }

            edgeContext = edgeContext with { ScopeNode = scopeNodeContext };
        }

        if (typeNode.ContainsOutgoingEdgeFor(edgeContext) is { } existingEdgeContext)
        {
            if (existingEdgeContext.ResolutionId != edgeContext.ResolutionId)
                typeNode.LinkResolutionIds(edgeContext.ResolutionId, existingEdgeContext.ResolutionId);
            return;
        }

        var scopeInstanceLevel = edgeContext.ScopeNode.CheckTypeProperties.GetScopeLevelFor(typeNode.Type);
        typeNode.RegisterScopeInstanceConfiguration(scopeInstanceLevel, edgeContext.ScopeNode);

        if (scopeInstanceLevel is not ScopeLevel.None)
        {
            switch (scopeInstanceLevel)
            {
                case ScopeLevel.Scope:
                    switch (edgeContext.ScopeNode)
                    {
                        case ScopeNodeContext.Container:
                            scopeNodeManager.RegisterContainerInstance(typeNode);
                            break;
                        case ScopeNodeContext.Scope { ScopeName: var scopeName }:
                            scopeNodeManager.RegisterScopedInstance(scopeName, typeNode);
                            break;
                        case ScopeNodeContext.TransientScope { TransientScopeName: var transientScopeName }:
                            scopeNodeManager.RegisterScopedInstance(transientScopeName, typeNode);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ScopeLevel.TransientScope:
                    switch (edgeContext.ScopeNode)
                    {
                        case ScopeNodeContext.Container:
                            scopeNodeManager.RegisterContainerInstance(typeNode);
                            break;
                        case ScopeNodeContext.Scope { TransientScopeName: {} transientScopeName }:
                            scopeNodeManager.RegisterScopedInstance(transientScopeName, typeNode);
                            break;
                        case ScopeNodeContext.Scope { TransientScopeName: null }:
                            scopeNodeManager.RegisterContainerInstance(typeNode);
                            break;
                        case ScopeNodeContext.TransientScope { TransientScopeName: var transientScopeName }:
                            scopeNodeManager.RegisterScopedInstance(transientScopeName, typeNode);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ScopeLevel.Container:
                    scopeNodeManager.RegisterContainerInstance(typeNode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (edgeContext.ScopeNode)
            {
                case ScopeNodeContext.Container:
                    scopeNodeManager.RegisterContainerInstance(typeNode);
                    break;
                case ScopeNodeContext.Scope { ScopeName: var scopeName }:
                    scopeNodeManager.RegisterScopedInstance(scopeName, typeNode);
                    break;
                case ScopeNodeContext.TransientScope { TransientScopeName: var transientScopeName }:
                    scopeNodeManager.RegisterScopedInstance(transientScopeName, typeNode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        switch (typeNodeType)
        {
            case not null when edgeContext.Override is OverrideContext.Any any && any.Overrides.Contains(typeNodeType, CustomSymbolEqualityComparer.IncludeNullability):
                resolutionSteps.OverrideStep(typeNode, edgeContext);
                break;
            case INamedTypeSymbol { Name: "IEnumerable" } enumerableType when CustomSymbolEqualityComparer.IncludeNullability.Equals(typeNodeType.OriginalDefinition, wellKnownTypesCollections.IEnumerable1):
                resolutionSteps.EnumerableStep(enumerableType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case IArrayTypeSymbol arrayType:
                resolutionSteps.EnumerableStep(arrayType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol { Name: "IAsyncEnumerable" } asyncEnumerableType when CustomSymbolEqualityComparer.IncludeNullability.Equals(typeNodeType.OriginalDefinition, wellKnownTypesCollections.IAsyncEnumerable1):
                resolutionSteps.AsyncEnumerableStep(asyncEnumerableType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol { TypeArguments.Length: >= 1 } functor when typeSymbolUtility.IsFuncDelegate(functor):
                resolutionSteps.FunctorStep(functor, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceType:
                resolutionSteps.InterfaceStep(interfaceType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol taskType when
                CustomSymbolEqualityComparer.Default.Equals(taskType.OriginalDefinition, wellKnownTypes.Task1)
                || wellKnownTypes.ValueTask1 is {} valueTask1 && CustomSymbolEqualityComparer.Default.Equals(taskType.OriginalDefinition, valueTask1):
                resolutionSteps.TaskStep(taskType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol keyValuePairType when CustomSymbolEqualityComparer.Default.Equals(keyValuePairType.OriginalDefinition, wellKnownTypesCollections.KeyValuePair2):
                resolutionSteps.KeyValuePairStep(keyValuePairType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } implementationType:
                resolutionSteps.ImplementationStep(implementationType, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            default:
                resolutionSteps.DefaultStep(typeNode, edgeContext, currentResolvedLocation);
                break;
        }
    }

    public void AssignFunctions()
    {
        var allTypeNodes = typeNodeManager.AllTypeNodes.ToImmutableArray();
        foreach (var typeNode in allTypeNodes)
        {
            var concreteSyncEdges = typeNode.OutgoingConcreteEdges.OfType<ConcreteSyncEdge>().ToImmutableArray();
            var concreteAsyncEdges = typeNode.OutgoingConcreteEdges.OfType<ConcreteAsyncEdge>().ToImmutableArray();
            var typeNodeIsNotWrappedIntoATask =
                !CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.ValueTask1) 
                || !CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.Task1);
            if (concreteSyncEdges.Length > 0)
            {
                var syncIncomingEdges = typeNode.Incoming.Where(e => e.Contexts.Any(c => !_asyncResolutionIds.Contains(c.ResolutionId))).ToImmutableArray();
                if (// if multiple incoming edges, but current type not wrapped in a task
                    syncIncomingEdges.Length > 1 /* Multiple incoming contexts */ && typeNodeIsNotWrappedIntoATask
                    // or any incoming edge is from a concrete functor (Func, Lazy, ThreadLocal)
                    || syncIncomingEdges.Any(e => e.SourceAsNode is ConcreteFunctorNode)
                    // or any outgoing edges contain concrete enumerable
                    || concreteSyncEdges.Any(e => e.Target is ConcreteEnumerableNodeBase)
                    // or Type Node is scope instance in some configurations
                    || typeNode.ScopeInstanceConfiguration.Any(kvp => kvp.Key is not ScopeLevel.None)
                    // or Type Node is scope root in some configurations
                    || typeNode.ScopeRootConfiguration.Count > 0)
                    NewFunctionIfNotAlready(typeNode, sync: true);
            }
            if (concreteAsyncEdges.Length > 0)
            {
                var asyncIncomingEdges = typeNode.Incoming.Where(e => e.Contexts.Any(c => _asyncResolutionIds.Contains(c.ResolutionId))).ToImmutableArray();
                if (// if multiple incoming edges, but current type not wrapped in a task
                    asyncIncomingEdges.Length > 1 /* Multiple incoming contexts */ && typeNodeIsNotWrappedIntoATask
                    // In async mode, if current type not wrapped in a task, but any incoming edge lead to a task node
                    || (asyncIncomingEdges.Any(e => e.SourceAsNode is ConcreteTaskNode) /* Any incoming node a task node */ && typeNodeIsNotWrappedIntoATask)
                    // or any incoming edge is from a concrete functor (Func, Lazy, ThreadLocal)
                    || asyncIncomingEdges.Any(e => e.SourceAsNode is ConcreteFunctorNode)
                    // or any outgoing edges contain concrete enumerable
                    || concreteSyncEdges.Any(e => e.Target is ConcreteEnumerableNodeBase)
                    // or Type Node is scope instance in some configurations
                    || typeNode.ScopeInstanceConfiguration.Any(kvp => kvp.Key is not ScopeLevel.None)
                    // or Type Node is scope root in some configurations
                    || typeNode.ScopeRootConfiguration.Count > 0)
                    NewFunctionIfNotAlready(typeNode, sync: false);
            }
        }

        foreach (var concreteEntryFunctionNode in concreteEntryFunctionNodeManager.AllNodes)
        {
            if (concreteEntryFunctionNode.ReturnType is { Target: { } rootTypeNode })
            {
                var function = functionFactory(rootTypeNode, true);
                _functions.Add(function);
                rootTypeNode.SyncFunction = function;
            }
        }

        return;

        void NewFunctionIfNotAlready(TypeNode typedInjectionNode, bool sync)
        {
            if (sync && typedInjectionNode.SyncFunction is null || !sync && typedInjectionNode.AsyncFunction is null)
            {
                var function = functionFactory(typedInjectionNode, sync);
                _functions.Add(function);
                if (sync)
                    typedInjectionNode.SyncFunction = function;
                else
                    typedInjectionNode.AsyncFunction = function;
            }
        }
    }
}