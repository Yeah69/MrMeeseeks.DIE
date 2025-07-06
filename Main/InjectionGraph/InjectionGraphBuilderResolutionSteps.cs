using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal class InjectionGraphBuilderResolutionSteps(
    IContainerCheckTypeProperties containerCheckTypeProperties,
    ILocalDiagLogger containerDiagLogger,
    IInjectablePropertyExtractor injectablePropertyExtractor,
    IdRegister idRegister,
    ConcreteImplementationNodeManager concreteImplementationNodeManager,
    ConcreteInterfaceNodeManager concreteInterfaceNodeManager,
    ConcreteEnumerableNodeManager concreteEnumerableNodeManager,
    ConcreteFunctorNodeManager concreteFunctorNodeManager,
    ConcreteOverrideNodeManager concreteOverrideNodeManager,
    ConcreteKeyValuePairNodeManager concreteKeyValuePairNodeManager,
    OverrideContextManager overrideContextManager,
    Lazy<ConcreteExceptionNode> concreteExceptionNode,
    Func<TypeNode, IConcreteNode, ConcreteEdge> concreteEdgeFactory)
{
    internal void OverrideStep(TypeNode typeNode, EdgeContext edgeContext)
    {
        var typeNodeType = typeNode.Type;
        var concreteOverrideNodeData = new ConcreteOverrideNodeData(typeNodeType);
        var concreteOverrideNode = concreteOverrideNodeManager.GetOrAddNode(concreteOverrideNodeData);
        ConnectToTypeNodeIfNotAlready(concreteOverrideNode, edgeContext, typeNode);
    }

    internal void FunctorStep(
        INamedTypeSymbol maybeFunctor,
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        var concreteFunctorNodeData = new ConcreteFunctorNodeData(maybeFunctor);
        var concreteFunctorNode = concreteFunctorNodeManager.GetOrAddNode(concreteFunctorNodeData);
        var newOverrideContext = overrideContextManager.GetOrAddContext(concreteFunctorNode.FunctorParameterTypes);
        var newEdgeContext = edgeContext with { Override = newOverrideContext };
        ConnectToTypeNodeIfNotAlready(concreteFunctorNode, newEdgeContext, typeNode);
        foreach (var (node, location) in concreteFunctorNode.ConnectIfNotAlready(newEdgeContext))
            queue.Enqueue(new ResolutionStep(
                node,
                newEdgeContext,
                location.Equals(Location.None) ? currentResolvedLocation : location));
    }

    internal void InterfaceStep(
        INamedTypeSymbol currentType,
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        if (GetConcreteImplementationType(out var isDefaultCase) is not {} implementation)
            return;

        var decorationSequence = containerCheckTypeProperties.GetDecorationSequenceFor(currentType, implementation);
        
        var concreteInterfaceNodeData = new ConcreteInterfaceNodeData(Interface: currentType);

        var concreteInterfaceNodeImplementationData = new ConcreteInterfaceNodeImplementationData(
            Implementation: implementation,
            Decorations: decorationSequence);

        var concreteInterfaceNode = concreteInterfaceNodeManager.GetOrAddNode(concreteInterfaceNodeData);
        
        ConnectToTypeNodeIfNotAlready(concreteInterfaceNode, edgeContext, typeNode);
        
        var newConnections = edgeContext.Key is KeyContext.Single { Value: var keyValue }
            ? concreteInterfaceNode.ConnectIfNotAlready(edgeContext, concreteInterfaceNodeImplementationData, isDefaultInjection: false, keyObject: keyValue)
            : concreteInterfaceNode.ConnectIfNotAlready(edgeContext, concreteInterfaceNodeImplementationData, isDefaultInjection: isDefaultCase, keyObject: null);
        
        foreach (var (node, location) in newConnections)
            queue.Enqueue(new ResolutionStep(
                node, 
                edgeContext,
                location.Equals(Location.None) ? currentResolvedLocation : location));
        return;

        INamedTypeSymbol? GetConcreteImplementationType(out bool isDefaultCase)
        {
            isDefaultCase = true;
            var currentOutwardFacingTypeId = idRegister.GetOutwardFacingTypeId(currentType);
            // If the context has an initial case ID for the matching outward facing type ID, we try to resolve the type by that ID.
            if (edgeContext.InitialInitialCaseChoice is InitialCaseChoiceContext.Single { OutwardFacingTypeId: var outwardId, InitialCaseId: var caseId }
                && currentOutwardFacingTypeId == outwardId)
            {
                isDefaultCase = false;
                var registeredImplementation = idRegister.GetTypeByInitialCaseId(edgeContext.Domain, caseId);
                if (registeredImplementation is INamedTypeSymbol namedTypeSymbol)
                    return namedTypeSymbol;
                ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
                containerDiagLogger.Error(
                    ErrorLogData.ResolutionException(
                        "Interface: ID registry didn't find type for a given initial case ID.",
                        currentType,
                        ImmutableStack<INamedTypeSymbol>.Empty), 
                    currentResolvedLocation);
                return null;
            }
            
            // If there is a registered composite type for the current interface type, we use that as the implementation.
            if (containerCheckTypeProperties.ShouldBeComposite(currentType) 
                && containerCheckTypeProperties.GetCompositeFor(currentType) is { } compositeType)
                return compositeType;
            
            // Otherwise, we try to resolve the type by the registered implementations.
            var key = edgeContext.Key is KeyContext.Single { Type: var keyType, Value: var keyValue } && !containerCheckTypeProperties.IsContextPassingType(currentType)
                ? new InjectionKey(keyType, keyValue)
                : null;
            var implementationResult = containerCheckTypeProperties.MapToSingleFittingImplementation(currentType, key);
            if (implementationResult is not ImplementationResult.Single { Implementation: { } singleImplementation })
            {
                ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
                var logMessage = implementationResult switch
                {
                    ImplementationResult.None => $"Interface: No implementation registered for \"{currentType.FullName()}\".",
                    ImplementationResult.Multiple { Implementations: var implementations} => $"Interface: Multiple implementations registered for \"{currentType.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                    _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
                };
                containerDiagLogger.Error(
                    ErrorLogData.ResolutionException(
                        logMessage,
                        currentType,
                        ImmutableStack<INamedTypeSymbol>.Empty), 
                    currentResolvedLocation);
                return null;
            }
            return singleImplementation;
        }
    }

    internal void ImplementationStep(
        INamedTypeSymbol currentType,
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        var key = edgeContext.Key is KeyContext.Single { Type: var keyType, Value: var keyValue } && !containerCheckTypeProperties.IsContextPassingType(currentType)
            ? new InjectionKey(keyType, keyValue)
            : null;
        var implementationResult = containerCheckTypeProperties.MapToSingleFittingImplementation(currentType, key);
        if (implementationResult is not ImplementationResult.Single { Implementation: { } implementation })
        {
            ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
            var logMessage = implementationResult switch
            {
                ImplementationResult.None => $"Class: No implementation registered for \"{currentType.FullName()}\".",
                ImplementationResult.Multiple { Implementations: var implementations} => $"Class: Multiple implementations registered for \"{currentType.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
            };
            containerDiagLogger.Error(
                ErrorLogData.ResolutionException(
                    logMessage,
                    currentType,
                    ImmutableStack<INamedTypeSymbol>.Empty), 
                currentResolvedLocation);
            return;
        }
        
        // Constructor
        var constructorResult = containerCheckTypeProperties.GetConstructorChoiceFor(implementation);
        if (constructorResult is not ConstructorResult.Single { Constructor: {} constructor})
        {
            ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
            var logMessage = constructorResult switch
            {
                ConstructorResult.None => $"Class.Constructor: No visible constructor found for implementation {currentType.FullName()}",
                ConstructorResult.Multiple => $"Class.Constructor: More than one visible constructor found for implementation {currentType.FullName()}",
                ConstructorResult.ChoiceFailedNone => $"Class.Constructor: Constructor choice didn't match with any constructor for implementation {currentType.FullName()}",
                ConstructorResult.ChoiceFailedMultiple => $"Class.Constructor: Constructor choice matched with multiple constructors for implementation {currentType.FullName()}",
                _ => throw new InvalidOperationException("Unexpected ConstructorResult")
            };
            containerDiagLogger.Error(
                ErrorLogData.ResolutionException(
                    logMessage,
                    currentType,
                    ImmutableStack<INamedTypeSymbol>.Empty), 
                currentResolvedLocation);
            return;
        }
        
        // Properties
        IReadOnlyList<IPropertySymbol> properties;
        if (containerCheckTypeProperties.GetPropertyChoicesFor(implementation) is { } propertyChoice)
            properties = propertyChoice;
        // Automatic property injection is disabled for record types, but property choices are still allowed
        else if (!implementation.IsRecord)
            properties = injectablePropertyExtractor
                .GetInjectableProperties(implementation)
                // Check whether property is settable
                .Where(p => p.IsRequired || (p.SetMethod?.IsInitOnly ?? false))
                .ToList();
        else 
            properties = [];
        
        var concreteImplementationNodeData = new ConcreteImplementationNodeData(
            implementation,
            constructor,
            properties.OrderBy(p => p.Name).ToList());
        
        var concreteImplementationNode = concreteImplementationNodeManager.GetOrAddNode(concreteImplementationNodeData);
        
        ConnectToTypeNodeIfNotAlready(concreteImplementationNode, edgeContext, typeNode);
        
        foreach (var (node, location) in concreteImplementationNode.ConnectIfNotAlready(edgeContext))
            queue.Enqueue(new ResolutionStep(
                node, 
                edgeContext,
                location.Equals(Location.None) ? currentResolvedLocation : location));
                
    }

    internal void KeyValuePairStep(
        INamedTypeSymbol currentType,
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        var concreteKeyValuePairNodeData = new ConcreteKeyValuePairNodeData(currentType);
        
        var concreteKeyValuePairNode = concreteKeyValuePairNodeManager.GetOrAddNode(concreteKeyValuePairNodeData);
        
        ConnectToTypeNodeIfNotAlready(concreteKeyValuePairNode, edgeContext, typeNode);
        
        foreach (var (node, location) in concreteKeyValuePairNode.ConnectIfNotAlready(edgeContext))
            queue.Enqueue(new ResolutionStep(
                node, 
                edgeContext,
                location.Equals(Location.None) ? currentResolvedLocation : location));
    }

    internal void EnumerableStep(
        ITypeSymbol currentType,
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        var concreteEnumerableNodeData = new ConcreteEnumerableNodeData(Enumerable: currentType);

        var concreteEnumerableNode = concreteEnumerableNodeManager.GetOrAddNode(concreteEnumerableNodeData);
        var unwrappedInnerType = concreteEnumerableNode.UnwrappedInnerType as INamedTypeSymbol 
                                 ?? throw new InvalidOperationException($"Unwrapped inner type of enumerable node {concreteEnumerableNodeData} is not a named type symbol, but {concreteEnumerableNode.UnwrappedInnerType.GetType().FullName}.");
        
        var outwardFacingTypeId = idRegister.GetOutwardFacingTypeId(unwrappedInnerType);
        var sequence = (concreteEnumerableNode switch
        {
            { KeyType: { } keyTypeSingular, IsKeyedMultiple: false } => containerCheckTypeProperties.MapToKeyedImplementations(unwrappedInnerType, keyTypeSingular)
                .Select(ConcreteEnumerableYield (kvp) => new ConcreteEnumerableYield.Key(keyTypeSingular, kvp.Key)),
            { KeyType: { } keyTypeMultiple, IsKeyedMultiple: true } => containerCheckTypeProperties
                .MapToKeyedMultipleImplementations(unwrappedInnerType, keyTypeMultiple)
                .Select(kvp => new ConcreteEnumerableYield.Key(keyTypeMultiple, kvp.Key)),
            _ => containerCheckTypeProperties.MapToImplementations(unwrappedInnerType, edgeContext.Key 
                    is KeyContext.Single { Type: var keyType, Value: var keyValue} ? new InjectionKey(keyType, keyValue) : null)
                .Select(i => new ConcreteEnumerableYield.Case(outwardFacingTypeId, idRegister.GetInitialCaseId(edgeContext.Domain, i)))
        }).ToArray();
        
        var concreteEnumerableNodeSequenceData = new ConcreteEnumerableNodeSequenceData(sequence);

        ConnectToTypeNodeIfNotAlready(concreteEnumerableNode, edgeContext, typeNode);
        
        foreach (var (node, newEdgeContext, location) in concreteEnumerableNode.ConnectIfNotAlready(edgeContext, concreteEnumerableNodeSequenceData))
            queue.Enqueue(new ResolutionStep(
                node, 
                newEdgeContext,
                location.Equals(Location.None) ? currentResolvedLocation : location));
    }

    internal void DefaultStep(TypeNode typeNode, EdgeContext edgeContext, Location currentResolvedLocation)
    {
        ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
        containerDiagLogger.Error(
            ErrorLogData.ResolutionException(
                $"Type {typeNode.Type.FullName()} could not be resolved.",
                typeNode.Type,
                ImmutableStack<INamedTypeSymbol>.Empty), 
            currentResolvedLocation);
    }
        
    private void ConnectToTypeNodeIfNotAlready(IConcreteNode concreteNode, EdgeContext edgeContextToContinueWith, TypeNode typeNode)
    {
        if (!typeNode.TryGetOutgoingEdgeFor(concreteNode, out var existingEdge))
        {
            existingEdge = concreteEdgeFactory(typeNode, concreteNode);
            typeNode.AddOutgoing(existingEdge);
        }

        existingEdge.AddContext(edgeContextToContinueWith);
    }
}