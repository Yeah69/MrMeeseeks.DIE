using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.Logging;
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
    ConcreteFunctorNodeManager concreteFunctorNodeManager,
    OverrideContextManager overrideContextManager,
    ScopeNodeContext.Container containerScopeNodeContext,
    ScopeNodeManager scopeNodeManager,
    ResolutionRegister resolutionRegister,
    EdgeRegistry edgeRegistry,
    TypeSymbolUtility typeSymbolUtility,
    AsyncAdjustments asyncAdjustments,
    ConnectedResolutionIdsRegistry  connectedResolutionIdsRegistry,
    LocalDiagLogger localDiagLogger,
    Func<TypeNode, bool, TypeNodeFunction> functionFactory,
    Func<TypeNode, IConcreteNode, ConcreteAsyncEdge> concreteAsyncEdgeFactory,
    Func<(TypeNode, TypeNode), TypeTypeEdge> typeTypeEdgeFactory,
    WellKnownTypes wellKnownTypes,
    WellKnownTypesCollections wellKnownTypesCollections)
    : IInjectionGraphBuilder, IContainerInstance
{
    private readonly List<ITypeNodeFunction> _functions = [];
    private ImmutableHashSet<int> _asyncResolutionIds = [];

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
        while (queue.Count > 0)
        {
            var (typeNode, edgeContext, currentResolvedLocation) = queue.Dequeue();
            MakeResolutionStep(typeNode, edgeContext, queue, currentResolvedLocation);
        }
    }

    public void SplitAsyncConcreteEdges()
    {
        _asyncResolutionIds =
        [
            .. asyncAdjustments.AwaitedNodes.Values.SelectMany(v => v).Distinct().SelectMany(connectedResolutionIdsRegistry.GetConnectedResolutionIdsAndSelf)
        ];

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
        
        foreach (var typeNode in typeNodeManager.AllTypeNodes)
            typeNode.SplitScopeBasedConfigurations();

        var faultyEntryFunctions = concreteEntryFunctionNodeManager.AllNodes
            .Where(n => !typeSymbolUtility.IsAsyncWrapperType(n.Data.ReturnType) && n.ReturnType.Target.Outgoing
                .OfType<ConcreteAsyncEdge>()
                .SelectMany(ae => ae.Contexts)
                .Any(aec => n.ReturnType.Contexts.Any(c => c.ResolutionId == aec.ResolutionId)));
        
        foreach (var concreteEntryFunctionNode in faultyEntryFunctions)
        {
            localDiagLogger.Error(ErrorLogData.FunctionHasToReturnAnAsyncTypeCompilationError(
                    $"Entry function with return type {concreteEntryFunctionNode.Data.ReturnType.FullName()} needs to be wrapped into an async type."),
                Location.None);
        }

        var faultyFunctors = concreteFunctorNodeManager.AllNodes
            .Where(n => 
                !typeSymbolUtility.IsAsyncWrapperType(n.ReturnedElement.Target.Type) 
                && n.ReturnedElement.Target.Outgoing
                    .OfType<ConcreteAsyncEdge>()
                    .SelectMany(ae => ae.Contexts)
                    .Any(aec => n.ReturnedElement.Contexts.Any(c => c.ResolutionId == aec.ResolutionId)));
        
        foreach (var concreteFunctorNode in faultyFunctors)
        {
            localDiagLogger.Error(ErrorLogData.FunctionHasToReturnAnAsyncTypeCompilationError(
                    $"Func-injection with return type {concreteFunctorNode.Data.Type.FullName()} needs to be wrapped into an async type."),
                Location.None);
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
            if (existingEdgeContext.ResolutionId != edgeContext.ResolutionId
                && !CustomSymbolEqualityComparer.Default.Equals(typeNodeType.OriginalDefinition, wellKnownTypes.ValueTask1)
                && !CustomSymbolEqualityComparer.Default.Equals(typeNodeType.OriginalDefinition, wellKnownTypes.Task1)
                && !CustomSymbolEqualityComparer.Default.Equals(typeNodeType.OriginalDefinition, wellKnownTypesCollections.IAsyncEnumerable1))
                connectedResolutionIdsRegistry.RegisterConnection(existingEdgeContext.ResolutionId, edgeContext.ResolutionId);
            return;
        }

        var scopeInstanceLevel = edgeContext.ScopeNode.CheckTypeProperties.GetScopeLevelFor(typeNode.Type);
        typeNode.RegisterScopeInstanceConfiguration(scopeInstanceLevel, edgeContext.ScopeNode);
        
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
            var outgoingSyncEdges = typeNode.OutgoingConcreteEdges.OfType<ConcreteSyncEdge>().ToImmutableArray<IEdge>();
            var outgoingAsyncEdges = typeNode.OutgoingConcreteEdges.OfType<ConcreteAsyncEdge>().ToImmutableArray<IEdge>();
            var typeNodeIsNotWrappedIntoATask =
                !CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.ValueTask1) 
                || !CustomSymbolEqualityComparer.Default.Equals(typeNode.Type.OriginalDefinition, wellKnownTypes.Task1);
            if (outgoingSyncEdges.Length > 0)
            {
                var incomingSyncEdges = typeNode.Incoming.Where(e => e.Contexts.Any(c => !_asyncResolutionIds.Contains(c.ResolutionId))).ToImmutableArray();
                if (CommonCriteria(incomingSyncEdges, outgoingSyncEdges, typeNode.SyncScopeInstanceConfiguration, typeNode.SyncScopeRootConfiguration))
                    NewFunctionIfNotAlready(typeNode, sync: true);
            }
            if (outgoingAsyncEdges.Length > 0)
            {
                var incomingAsyncEdges = typeNode.Incoming.Where(e => e.Contexts.Any(c => _asyncResolutionIds.Contains(c.ResolutionId))).ToImmutableArray();
                if (CommonCriteria(incomingAsyncEdges, outgoingAsyncEdges, typeNode.AsyncScopeInstanceConfiguration, typeNode.AsyncScopeRootConfiguration)
                    // In async mode, if any outgoing edge lead to a task node
                    || incomingAsyncEdges.Any(e => e.SourceAsNode is ConcreteTaskNode) /* Any outgoing node a task node */)
                    NewFunctionIfNotAlready(typeNode, sync: false);
            }

            continue;

            bool CommonCriteria(
                ImmutableArray<IEdge> incomingEdges, 
                ImmutableArray<IEdge> outgoingEdges,
                IReadOnlyDictionary<ScopeLevel, HashSet<ScopeNodeContext>> scopeInstanceConfiguration,
                IReadOnlyDictionary<ScopeNodeContext, (HashSet<ScopeNodeContext> PreviousScopeNodeContexts, TypeTypeEdge? ScopeRootTypeTypeEdge)> scopeRootConfiguration) =>
                // if multiple incoming edges, but current type not wrapped in a task
                incomingEdges.Length > 1 /* Multiple incoming contexts */ && typeNodeIsNotWrappedIntoATask
                // or any incoming edge is from a concrete functor (Func, Lazy, ThreadLocal)
                || incomingEdges.Any(e => e.SourceAsNode is ConcreteFunctorNode)
                // or any outgoing edges contain concrete enumerable
                || outgoingEdges.Any(e => e.TargetAsNode is ConcreteEnumerableNodeBase)
                // or Type Node is scope instance in some configurations
                || scopeInstanceConfiguration.Any(kvp => kvp.Key is not ScopeLevel.None)
                // or Type Node is scope root in some configurations
                || scopeRootConfiguration.Count > 0;
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