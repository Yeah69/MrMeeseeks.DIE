using System.Collections.Concurrent;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

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

    // resolve unused method
    void BuildForAsyncGraph(SyncGraphRoot syncGraphRoot);
    void BuildForAsyncGraphNew(RawGraphRoot syncGraphRoot, HashSet<int> asyncResolutionIds);
    void BuildForSyncGraph(RawGraphRoot syncGraphRoot, HashSet<int> asyncResolutionIds);
    HashSet<int> GetSplitData();
    void AssignFunctions();
    void AddCrossGraphReferences(IGraphRoot otherGraphRoot);
}

internal sealed class InjectionGraphBuilder(
    InjectionGraphBuilderResolutionSteps resolutionSteps,
    TypeNodeManager typeNodeManager,
    ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
    ConcreteImplementationNodeManager concreteImplementationNodeManager,
    ConcreteInterfaceNodeManager concreteInterfaceNodeManager,
    ConcreteEnumerableNodeManager concreteEnumerableNodeManager,
    ConcreteFunctorNodeManager concreteFunctorNodeManager,
    ConcreteOverrideNodeManager concreteOverrideNodeManager,
    ConcreteKeyValuePairNodeManager concreteKeyValuePairNodeManager,
    ConcreteTaskNodeManager concreteTaskNodeManager,
    ConcreteExceptionNode concreteExceptionNode,
    OverrideContextManager overrideContextManager,
    ScopeNodeContext.Container containerScopeNodeContext,
    ScopeNodeManager scopeNodeManager,
    ResolutionRegister resolutionRegister,
    GraphTypeHolder graphTypeHolder,
    EdgeRegistry edgeRegistry,
    TypeSymbolUtility typeSymbolUtility,
    AsyncAdjustments asyncAdjustments,
    Func<TypeNode, TypeNodeFunction> functionFactory,
    Func<TypeNode, INamedTypeSymbol, AsyncTypeNodeFunction> asyncFunctionFactory,
    Func<ITypeNodeFunction, FunctionEdgeType> functionEdgeTypeFactory,
    WellKnownTypes wellKnownTypes,
    WellKnownTypesCollections wellKnownTypesCollections)
    : IInjectionGraphBuilder, IScopeInstance
{
    private readonly List<ITypeNodeFunction> _functions = [];

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
            resolutionRegister.GetNewResolutionId());
        var concreteEntryFunctionNodeData = new ConcreteEntryFunctionNodeData(entryFunctionName, rootType, overrides);
        var concreteEntryFunctionNode = concreteEntryFunctionNodeManager.GetOrAddNode(concreteEntryFunctionNodeData);
        var rootTypeNodes = concreteEntryFunctionNode.ConnectIfNotAlready(rootEdgeContext).Select(t => t.TypeNode).ToList();
        var queue = new Queue<ResolutionStep>();
        foreach (var rootTypeNode in rootTypeNodes)
            queue.Enqueue(new(rootTypeNode, rootEdgeContext, createFunctionAttributeLocation));
        while (queue.Count > 0)
        {
            var (typeNode, edgeContext, currentResolvedLocation) = queue.Dequeue();
            MakeResolutionStep(typeNode, edgeContext, queue, currentResolvedLocation);
        }
    }

    public void BuildForAsyncGraph(SyncGraphRoot syncGraphRoot)
    {
        var awaitedNodes = syncGraphRoot.AsyncAdjustments.AwaitedNodes;
        var asyncResolutionIds = Adjust();
        var syncEdges = syncGraphRoot.EdgeRegistry.Edges.ToImmutableArray();
        var nodesMap = new ConcurrentDictionary<INode, INode>();
        foreach (var syncEdge in syncEdges)
        {
            if (!syncEdge.Contexts.Any(c => asyncResolutionIds.Contains(c.ResolutionId)))
                continue;
            
            var syncSource = syncEdge.SourceAsNode;
            var syncTarget = syncEdge.TargetAsNode;
            var asyncSource = nodesMap.GetOrAdd(syncSource, Copy);
            var asyncTarget = nodesMap.GetOrAdd(syncTarget, Copy);

            var asyncContexts = syncEdge.Contexts.Where(c => asyncResolutionIds.Contains(c.ResolutionId));

            if (asyncSource is IConcreteNode sourceConcreteNode && asyncTarget is TypeNode)
                foreach (var asyncContext in asyncContexts)
                    switch (sourceConcreteNode)
                    {
                        case ConcreteEnumerableNode concreteEnumerableNode:
                            concreteEnumerableNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteFunctorNode concreteFunctorNode:
                            concreteFunctorNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteImplementationNode concreteImplementationNode:
                            concreteImplementationNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteInterfaceNode concreteInterfaceNode:
                            concreteInterfaceNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteKeyValuePairNode concreteKeyValuePairNode:
                            concreteKeyValuePairNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteEntryFunctionNode concreteEntryFunctionNode:
                            concreteEntryFunctionNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteTaskNode concreteTaskNode:
                            concreteTaskNode.ConnectIfNotAlready(asyncContext);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            else if (asyncSource is TypeNode sourceTypeNode && asyncTarget is IConcreteNode targetConcreteNode)
                foreach (var asyncContext in asyncContexts)
                    resolutionSteps.ConnectToTypeNodeIfNotAlready(targetConcreteNode, asyncContext, sourceTypeNode);

            continue;

            INode Copy(INode node) =>
                node switch
                {
                    ConcreteEnumerableNode { Data: { } data0 } => concreteEnumerableNodeManager.GetOrAddNode(data0),
                    ConcreteExceptionNode => concreteExceptionNode,
                    ConcreteFunctorNode { Data: { } data1 } => concreteFunctorNodeManager.GetOrAddNode(data1),
                    ConcreteImplementationNode { Data: { } data2 } => concreteImplementationNodeManager.GetOrAddNode(data2),
                    ConcreteInterfaceNode { Data: { } data3 } => concreteInterfaceNodeManager.GetOrAddNode(data3),
                    ConcreteKeyValuePairNode { Data: { } data4 } => concreteKeyValuePairNodeManager.GetOrAddNode(data4),
                    ConcreteOverrideNode { Data: { } data5 } => concreteOverrideNodeManager.GetOrAddNode(data5),
                    ConcreteEntryFunctionNode { Data: { } data6 } => concreteEntryFunctionNodeManager.GetOrAddNode(data6),
                    ConcreteTaskNode { Data: { } data7 } => concreteTaskNodeManager.GetOrAddNode(data7),
                    TypeNode { Type: { } type } => typeNodeManager.GetOrAddNode(type),
                    _ => throw new ArgumentOutOfRangeException(nameof(node))
                };
        }

        foreach (var syncEdge in syncEdges)
        {
            if (syncEdge.Contexts.Any(c => !asyncResolutionIds.Contains(c.ResolutionId)))
                continue;
            
            syncEdge.SourceAsNode.RemoveEdge(syncEdge);
            syncEdge.TargetAsNode.RemoveEdge(syncEdge);
            syncGraphRoot.EdgeRegistry.Unregister(syncEdge);
            
            Cleanup(syncEdge.SourceAsNode);
            Cleanup(syncEdge.TargetAsNode);
            continue;

            void Cleanup(INode node)
            {
                switch (node)
                {
                    case TypeNode { Outgoing.Count: 0 } typeNode:
                        syncGraphRoot.TypeNodeManager.RemoveNode(typeNode);
                        break;
                    case IConcreteNode { IncomingEdges.Count: > 0 }:
                        return;
                    case ConcreteEnumerableNode { Data: {} data }:
                        syncGraphRoot.ConcreteEnumerableNodeManager.RemoveNode(data);
                        break;
                    case ConcreteFunctorNode { Data: {} data0 }:
                        syncGraphRoot.ConcreteFunctorNodeManager.RemoveNode(data0);
                        break;
                    case ConcreteImplementationNode { Data: {} data1 }:
                        syncGraphRoot.ConcreteImplementationNodeManager.RemoveNode(data1);
                        break;
                    case ConcreteInterfaceNode { Data: {} data2 }:
                        syncGraphRoot.ConcreteInterfaceNodeManager.RemoveNode(data2);
                        break;
                    case ConcreteKeyValuePairNode { Data: {} data3 }:
                        syncGraphRoot.ConcreteKeyValuePairNodeManager.RemoveNode(data3);
                        break;
                    case ConcreteOverrideNode { Data: {} data4 }:
                        syncGraphRoot.ConcreteOverrideNodeManager.RemoveNode(data4);
                        break;
                    case ConcreteEntryFunctionNode { Data: {} data5 }:
                        syncGraphRoot.ConcreteEntryFunctionNodeManager.RemoveNode(data5);
                        break;
                    case ConcreteTaskNode { Data: {} data6 }:
                        syncGraphRoot.ConcreteTaskNodeManager.RemoveNode(data6);
                        break;
                }
            }
        }

        syncEdges = syncGraphRoot.EdgeRegistry.Edges.ToImmutableArray();
        var asdf = edgeRegistry;
        foreach (var syncEdge in syncEdges)
        {
            if (syncEdge is TypeEdge { Target: { Type: { } targetType } typeNode } typeEdge
                && !syncGraphRoot.TypeNodeManager.AllTypeNodes.Contains(typeNode)
                && typeNodeManager.TryGetNode(targetType, out var asyncTypeNode))
            {
                typeEdge.ReplaceTarget(asyncTypeNode);
            }
        }
        return;

        HashSet<int> Adjust()
        {
            EnhanceAwaitedNodes();
            var set = new HashSet<int>();
            foreach (var resolutionIds in awaitedNodes.Select(kvp => kvp.Value))
                set.UnionWith(resolutionIds);
            return set;

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
                                .OfType<ConcreteEdge>()
                                .Select(e => e.Source)
                                .Where(n => !visitedNodes.Contains(n));
                            foreach (var typeNode in sequence)
                                innerQueue.Enqueue(typeNode);
                        } 
                        else if (currentNode is TypeNode currentTypeNode)
                        {
                            var sequence = currentTypeNode
                                .IncomingEdges
                                .Where(e => e.Contexts.Any(c => c.ResolutionId == resolutionId))
                                .OfType<TypeEdge>()
                                .Select(e => e.Source)
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

    public void BuildForAsyncGraphNew(RawGraphRoot rawGraphRoot, HashSet<int> asyncResolutionIds)
    {
        var rawEdges = rawGraphRoot.EdgeRegistry.Edges.ToImmutableArray();
        var nodesMap = new ConcurrentDictionary<INode, INode>();
        foreach (var rawEdge in rawEdges)
        {
            if (!rawEdge.Contexts.Any(c => asyncResolutionIds.Contains(c.ResolutionId)))
                continue;
            
            var rawSource = rawEdge.SourceAsNode;
            var rawTarget = rawEdge.TargetAsNode;
            var asyncSource = nodesMap.GetOrAdd(rawSource, Copy);
            var asyncTarget = nodesMap.GetOrAdd(rawTarget, Copy);
            
            if (rawSource is TypeNode rawSourceTypeNode && asyncSource is TypeNode asyncSourceTypeNode)
                AdjustTypeNode(rawSourceTypeNode, asyncSourceTypeNode);
            if (rawTarget is TypeNode rawTargetTypeNode && asyncSource is TypeNode asyncTargetTypeNode)
                AdjustTypeNode(rawTargetTypeNode, asyncTargetTypeNode);

            var asyncContexts = rawEdge.Contexts.Where(c => asyncResolutionIds.Contains(c.ResolutionId));

            if (asyncSource is IConcreteNode sourceConcreteNode && asyncTarget is TypeNode)
                foreach (var asyncContext in asyncContexts)
                    switch (sourceConcreteNode)
                    {
                        case ConcreteEnumerableNode concreteEnumerableNode:
                            concreteEnumerableNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteFunctorNode concreteFunctorNode:
                            concreteFunctorNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteImplementationNode concreteImplementationNode:
                            concreteImplementationNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteInterfaceNode concreteInterfaceNode:
                            concreteInterfaceNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteKeyValuePairNode concreteKeyValuePairNode:
                            concreteKeyValuePairNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteEntryFunctionNode concreteEntryFunctionNode:
                            concreteEntryFunctionNode.ConnectIfNotAlready(asyncContext);
                            break;
                        case ConcreteTaskNode concreteTaskNode:
                            concreteTaskNode.ConnectIfNotAlready(asyncContext);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            else if (asyncSource is TypeNode sourceTypeNode && asyncTarget is IConcreteNode targetConcreteNode)
                foreach (var asyncContext in asyncContexts)
                    resolutionSteps.ConnectToTypeNodeIfNotAlready(targetConcreteNode, asyncContext, sourceTypeNode);

            continue;

            INode Copy(INode node) =>
                node switch
                {
                    ConcreteEnumerableNode { Data: { } data0 } => concreteEnumerableNodeManager.GetOrAddNode(data0),
                    ConcreteExceptionNode => concreteExceptionNode,
                    ConcreteFunctorNode { Data: { } data1 } => concreteFunctorNodeManager.GetOrAddNode(data1),
                    ConcreteImplementationNode { Data: { } data2 } => concreteImplementationNodeManager.GetOrAddNode(data2),
                    ConcreteInterfaceNode { Data: { } data3 } => concreteInterfaceNodeManager.GetOrAddNode(data3),
                    ConcreteKeyValuePairNode { Data: { } data4 } => concreteKeyValuePairNodeManager.GetOrAddNode(data4),
                    ConcreteOverrideNode { Data: { } data5 } => concreteOverrideNodeManager.GetOrAddNode(data5),
                    ConcreteEntryFunctionNode { Data: { } data6 } => concreteEntryFunctionNodeManager.GetOrAddNode(data6),
                    ConcreteTaskNode { Data: { } data7 } => concreteTaskNodeManager.GetOrAddNode(data7),
                    TypeNode { Type: { } type } => typeNodeManager.GetOrAddNode(type),
                    _ => throw new ArgumentOutOfRangeException(nameof(node))
                };

            // todo unify the sync version with this
            void AdjustTypeNode(TypeNode rawTypeNode, TypeNode copyTypeNode)
            {
                foreach (var keyValuePair in rawTypeNode.ScopeInstanceConfiguration)
                    foreach (var scopeNodeContext in keyValuePair.Value)
                        copyTypeNode.RegisterScopeInstanceConfiguration(keyValuePair.Key, scopeNodeContext);

                foreach (var keyValuePair in rawTypeNode.ScopeRootConfiguration)
                    foreach (var scopeNodeContext in keyValuePair.Value)
                        copyTypeNode.RegisterScopeRootConfiguration(keyValuePair.Key, scopeNodeContext);
            }
        }
    }

    // todo split into subclasses
    public void BuildForSyncGraph(RawGraphRoot rawGraphRoot, HashSet<int> asyncResolutionIds)
    {
        var rawEdges = rawGraphRoot.EdgeRegistry.Edges.ToImmutableArray();
        var nodesMap = new ConcurrentDictionary<INode, INode>();
        foreach (var rawEdge in rawEdges)
        {
            if (!rawEdge.Contexts.Any(c => !asyncResolutionIds.Contains(c.ResolutionId)))
                continue;
            
            var rawSource = rawEdge.SourceAsNode;
            var rawTarget = rawEdge.TargetAsNode;
            var syncSource = nodesMap.GetOrAdd(rawSource, Copy);
            var syncTarget = nodesMap.GetOrAdd(rawTarget, Copy);
            
            if (rawSource is TypeNode rawSourceTypeNode && syncSource is TypeNode syncSourceTypeNode)
                AdjustTypeNode(rawSourceTypeNode, syncSourceTypeNode);
            if (rawTarget is TypeNode rawTargetTypeNode && syncTarget is TypeNode syncTargetTypeNode)
                AdjustTypeNode(rawTargetTypeNode, syncTargetTypeNode);

            var syncContexts = rawEdge.Contexts.Where(c => !asyncResolutionIds.Contains(c.ResolutionId));

            if (syncSource is IConcreteNode sourceConcreteNode && syncTarget is TypeNode)
                foreach (var syncContext in syncContexts)
                    switch (sourceConcreteNode)
                    {
                        case ConcreteEnumerableNode concreteEnumerableNode:
                            concreteEnumerableNode.ConnectIfNotAlready(syncContext);
                            break;
                        case ConcreteFunctorNode concreteFunctorNode:
                            concreteFunctorNode.ConnectIfNotAlready(syncContext);
                            break;
                        case ConcreteImplementationNode concreteImplementationNode:
                            concreteImplementationNode.ConnectIfNotAlready(syncContext);
                            break;
                        case ConcreteInterfaceNode concreteInterfaceNode:
                            concreteInterfaceNode.ConnectIfNotAlready(syncContext);
                            break;
                        case ConcreteKeyValuePairNode concreteKeyValuePairNode:
                            concreteKeyValuePairNode.ConnectIfNotAlready(syncContext);
                            break;
                        case ConcreteEntryFunctionNode concreteEntryFunctionNode:
                            concreteEntryFunctionNode.ConnectIfNotAlready(syncContext);
                            break;
                        case ConcreteTaskNode concreteTaskNode:
                            concreteTaskNode.ConnectIfNotAlready(syncContext);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            else if (syncSource is TypeNode sourceTypeNode && syncTarget is IConcreteNode targetConcreteNode)
                foreach (var syncContext in syncContexts)
                    resolutionSteps.ConnectToTypeNodeIfNotAlready(targetConcreteNode, syncContext, sourceTypeNode);

            continue;

            INode Copy(INode node) =>
                node switch
                {
                    ConcreteEnumerableNode { Data: { } data0 } => concreteEnumerableNodeManager.GetOrAddNode(data0),
                    ConcreteExceptionNode => concreteExceptionNode,
                    ConcreteFunctorNode { Data: { } data1 } => concreteFunctorNodeManager.GetOrAddNode(data1),
                    ConcreteImplementationNode { Data: { } data2 } => concreteImplementationNodeManager.GetOrAddNode(data2),
                    ConcreteInterfaceNode { Data: { } data3 } => concreteInterfaceNodeManager.GetOrAddNode(data3),
                    ConcreteKeyValuePairNode { Data: { } data4 } => concreteKeyValuePairNodeManager.GetOrAddNode(data4),
                    ConcreteOverrideNode { Data: { } data5 } => concreteOverrideNodeManager.GetOrAddNode(data5),
                    ConcreteEntryFunctionNode { Data: { } data6 } => concreteEntryFunctionNodeManager.GetOrAddNode(data6),
                    ConcreteTaskNode { Data: { } data7 } => concreteTaskNodeManager.GetOrAddNode(data7),
                    TypeNode { Type: { } type } => typeNodeManager.GetOrAddNode(type),
                    _ => throw new ArgumentOutOfRangeException(nameof(node))
                };

            void AdjustTypeNode(TypeNode rawTypeNode, TypeNode copyTypeNode)
            {
                foreach (var keyValuePair in rawTypeNode.ScopeInstanceConfiguration)
                    foreach (var scopeNodeContext in keyValuePair.Value)
                        copyTypeNode.RegisterScopeInstanceConfiguration(keyValuePair.Key, scopeNodeContext);

                foreach (var keyValuePair in rawTypeNode.ScopeRootConfiguration)
                    foreach (var scopeNodeContext in keyValuePair.Value)
                        copyTypeNode.RegisterScopeRootConfiguration(keyValuePair.Key, scopeNodeContext);
            }
        }
    }

    public void AddCrossGraphReferences(IGraphRoot otherGraphRoot)
    {
        var typeNodes = typeNodeManager.AllTypeNodes.ToImmutableArray();
        foreach (var typeNode in typeNodes)
        {
            if (otherGraphRoot.TypeNodeManager.TryGetNode(typeNode.Type, out var otherTypeNode))
            {
                var incomingEdges = typeNode.Incoming.ToImmutableArray();
                foreach (var incomingEdge in incomingEdges)
                {
                    var matchingContexts = incomingEdge.Contexts.Where(c => otherTypeNode.Outgoing.Any(e => e.Contexts.Contains(c)));
                    if (matchingContexts.Any())
                    {
                        incomingEdge.ReplaceTarget(otherTypeNode);
                        typeNode.RemoveEdge(incomingEdge);
                        edgeRegistry.Unregister(incomingEdge);
                    }
                }

                if (typeNode.Incoming.Count == 0)
                {
                    typeNodeManager.RemoveNode(typeNode);
                }
            }
        }
    }

    public HashSet<int> GetSplitData()
    {
        var awaitedNodes = asyncAdjustments.AwaitedNodes;
        return Adjust();

        HashSet<int> Adjust()
        {
            EnhanceAwaitedNodes();
            var set = new HashSet<int>();
            foreach (var resolutionIds in awaitedNodes.Select(kvp => kvp.Value))
                set.UnionWith(resolutionIds);
            return set;

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
                                .OfType<ConcreteEdge>()
                                .Select(e => e.Source)
                                .Where(n => !visitedNodes.Contains(n));
                            foreach (var typeNode in sequence)
                                innerQueue.Enqueue(typeNode);
                        } 
                        else if (currentNode is TypeNode currentTypeNode)
                        {
                            var sequence = currentTypeNode
                                .IncomingEdges
                                .Where(e => e.Contexts.Any(c => c.ResolutionId == resolutionId))
                                .OfType<TypeEdge>()
                                .Select(e => e.Source)
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
            var scopeRootContext = scopeNodeManager.GetScopeNodeContext(edgeContext.ScopeNode, typeNode, scopeRootLevel);

            typeNode.RegisterScopeRootConfiguration(scopeRootContext, edgeContext.ScopeNode);
                
            edgeContext = edgeContext with { ScopeNode = scopeRootContext };
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
            if (// if multiple incoming edges x contexts, but current type not wrapped in a task
                (typeNode.Incoming.SelectMany(i => i.Contexts).Skip(1).Any() /* Multiple incoming contexts */ 
                 && (!CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.ValueTask1) 
                     || !CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.Task1))) /* Type isn't wrapped into a task */
                // In async mode, if current type not wrapped in a task, but any incoming edge lead to a task node
                || (graphTypeHolder.Type is GraphType.Async
                    && typeNode.Incoming.Any(e => e.Source is ConcreteTaskNode) /* Any incoming node a task node */
                    && (!CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.ValueTask1) 
                        || !CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.Task1))) /* Type isn't wrapped into a task */
                // or any incoming edge is from a concrete functor (Func, Lazy, ThreadLocal)
                || typeNode.Incoming.Any(e => e.Source is ConcreteFunctorNode)
                // or any outgoing edges contain concrete enumerable
                || typeNode.Outgoing.Any(e => e.Target is ConcreteEnumerableNode)
                // or Type Node is scope instance in some configurations
                || typeNode.ScopeInstanceConfiguration.Any(kvp => kvp.Key is not ScopeLevel.None)
                // or Type Node is scope root in some configurations
                || typeNode.ScopeRootConfiguration.Count > 0)
                NewFunctionIfNotAlready(typeNode);

        foreach (var concreteEntryFunctionNode in concreteEntryFunctionNodeManager.AllNodes)
        {
            if (graphTypeHolder.Type is GraphType.Async
                && (CustomSymbolEqualityComparer.Default.Equals(concreteEntryFunctionNode.Data.ReturnType.OriginalDefinition, wellKnownTypes.ValueTask1)
                    || CustomSymbolEqualityComparer.Default.Equals(concreteEntryFunctionNode.Data.ReturnType.OriginalDefinition, wellKnownTypes.Task1)))
                continue;
            
            if (concreteEntryFunctionNode.ReturnType is { Target: { } rootTypeNode, Type: DefaultEdgeType } edge)
            {
                var function = CreateFunction(rootTypeNode);
                _functions.Add(function);
                edge.Type = functionEdgeTypeFactory(function);
            }
        }

        return;

        void NewFunctionIfNotAlready(TypeNode typedInjectionNode)
        {
            if (typedInjectionNode.Incoming.Any(e => e.Type is DefaultEdgeType))
            {
                var function = typedInjectionNode.Incoming
                    .Select(e => e.Type)
                    .OfType<FunctionEdgeType>()
                    .Select(fet => fet.Function)
                    .FirstOrDefault();
                if (function is null)
                {
                    function = CreateFunction(typedInjectionNode);
                    _functions.Add(function);
                }
                foreach (var incoming in typedInjectionNode.Incoming)
                    incoming.Type = functionEdgeTypeFactory(function);
            }
        }

        ITypeNodeFunction CreateFunction(TypeNode typedInjectionNode)
        {
            if (graphTypeHolder.Type is GraphType.Sync)
                return functionFactory(typedInjectionNode);
            
            var taskWrappedType = wellKnownTypes.ValueTask1 is not null
                ? wellKnownTypes.ValueTask1.Construct(typedInjectionNode.Type)
                : wellKnownTypes.Task1.Construct(typedInjectionNode.Type);
            return asyncFunctionFactory(typedInjectionNode, taskWrappedType);
        }
    }
}