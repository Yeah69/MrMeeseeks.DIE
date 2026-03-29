using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class InjectionGraphBuilderResolutionSteps(
    LocalDiagLogger containerDiagLogger,
    InjectablePropertyExtractor injectablePropertyExtractor,
    ConcreteImplementationNodeManager concreteImplementationNodeManager,
    ConcreteInterfaceNodeManager concreteInterfaceNodeManager,
    ConcreteEnumerableNodeManager concreteEnumerableNodeManager,
    ConcreteFunctorNodeManager concreteFunctorNodeManager,
    ConcreteOverrideNodeManager concreteOverrideNodeManager,
    ConcreteKeyValuePairNodeManager concreteKeyValuePairNodeManager,
    OverrideContextManager overrideContextManager,
    TypeSymbolUtility typeSymbolUtility,
    CheckIterableTypes checkIterableTypes,
    IdRegister idRegister,
    WellKnownTypesCollections wellKnownTypesCollections,
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
        var concreteInterfaceNodeData = new ConcreteInterfaceNodeData(Interface: currentType);

        var concreteInterfaceNode = concreteInterfaceNodeManager.GetOrAddNode(concreteInterfaceNodeData);
        
        ConnectToTypeNodeIfNotAlready(concreteInterfaceNode, edgeContext, typeNode);
        
        var connectionResult = concreteInterfaceNode.ConnectIfNotAlready(edgeContext);
        
        if (connectionResult is ConcreteInterfaceNode.CaseIdResponse.Success { TypeNode: var newNode, Location: var newLocation, EdgeContext: var newEdgeContext})
            queue.Enqueue(new ResolutionStep(
                newNode, 
                newEdgeContext, 
                newLocation.Equals(Location.None) ? currentResolvedLocation : newLocation));
        else if (connectionResult is ConcreteInterfaceNode.CaseIdResponse.Error { ErrorMessage: var errorMessage})
            containerDiagLogger.Error(
                ErrorLogData.ResolutionException(
                    errorMessage,
                    currentType,
                    ImmutableStack<INamedTypeSymbol>.Empty), 
                currentResolvedLocation);
    }

    internal void ImplementationStep(
        INamedTypeSymbol currentType,
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        var key = edgeContext.Key is KeyContext.Single { Type: var keyType, Value: var keyValue } 
                  && !edgeContext.ScopeNode.CheckTypeProperties.IsContextPassingType(currentType)
            ? new InjectionKey(keyType, keyValue)
            : null;
        var implementationResult = edgeContext.ScopeNode.CheckTypeProperties.MapToSingleFittingImplementation(currentType, key);
        if (implementationResult is not ImplementationResult.Single { Implementation: { } implementation })
        {
            ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
            var logMessage = implementationResult switch
            {
                ImplementationResult.None5 => $"Class: No implementation registered for \"{currentType.FullName()}\".",
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
        var constructorResult = edgeContext.ScopeNode.CheckTypeProperties.GetConstructorChoiceFor(implementation);
        if (constructorResult is not ConstructorResult.Single { Constructor: {} constructor})
        {
            ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext, typeNode);
            var logMessage = constructorResult switch
            {
                ConstructorResult.None6 => $"Class.Constructor: No visible constructor found for implementation {currentType.FullName()}",
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
        if (edgeContext.ScopeNode.CheckTypeProperties.GetPropertyChoicesFor(implementation) is { } propertyChoice)
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
        
        foreach (var (node, location, context) in concreteImplementationNode.ConnectIfNotAlready(edgeContext))
            queue.Enqueue(new ResolutionStep(
                node, 
                context,
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
        var concreteEnumerableNodeData = CreateData();

        var concreteEnumerableNode = concreteEnumerableNodeManager.GetOrAddNode(concreteEnumerableNodeData);

        ConnectToTypeNodeIfNotAlready(concreteEnumerableNode, edgeContext, typeNode);
        
        foreach (var (node, newEdgeContext, location) in concreteEnumerableNode.ConnectIfNotAlready(edgeContext))
            queue.Enqueue(new ResolutionStep(
                node, 
                newEdgeContext,
                location.Equals(Location.None) ? currentResolvedLocation : location));
        return;

        ConcreteEnumerableNodeData CreateData()
        {
            var maybeWrappedItemType = currentType switch
            {
                INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedType => namedType.TypeArguments[0],
                IArrayTypeSymbol arrayType => arrayType.ElementType,
                _ => throw new InvalidOperationException(
                    $"The enumerable type '{currentType}' is not supported. It must be a generic type with one type argument or an array type.")
            };
            ITypeSymbol? maybeKeyValuePairKeyType = null;
            var isKeyValuePairWithCollectionValue = false;
            var tempUnwrappedItemType = typeSymbolUtility.GetUnwrappedType(maybeWrappedItemType);
            if (CustomSymbolEqualityComparer.Default.Equals(tempUnwrappedItemType.OriginalDefinition,
                    wellKnownTypesCollections.KeyValuePair2)
                && tempUnwrappedItemType is INamedTypeSymbol { TypeArguments: [var keyType0, var valueType] })
            {
                maybeKeyValuePairKeyType = keyType0;
                tempUnwrappedItemType = typeSymbolUtility.GetUnwrappedType(valueType);
                isKeyValuePairWithCollectionValue = checkIterableTypes.IsCollectionType(tempUnwrappedItemType);
            }

            var unwrappedItemType = tempUnwrappedItemType;
            
            // KeyValuePair involved
            if (maybeKeyValuePairKeyType is {} keyValuePairKeyType && unwrappedItemType is INamedTypeSymbol { TypeKind: TypeKind.Interface } namedUnwrappedItemType)
            {
                var keyValues = (isKeyValuePairWithCollectionValue 
                    ? edgeContext.ScopeNode.CheckTypeProperties.MapToKeyedMultipleImplementations(namedUnwrappedItemType, keyValuePairKeyType).Select(kvp => kvp.Key)
                    : edgeContext.ScopeNode.CheckTypeProperties.MapToKeyedImplementations(namedUnwrappedItemType, keyValuePairKeyType).Select(kvp => kvp.Key))
                    .ToImmutableArray();

                var keyResult = new ConcreteEnumerableNodeData.Key(currentType, maybeWrappedItemType, keyValuePairKeyType, keyValues,
                    edgeContext is { Key: not KeyContext.None1 } or { CaseChoice: not CaseChoiceContext.None2 });
                
                return keyResult;
            }
            var injectionKey = edgeContext.Key is KeyContext.Single { Type: var keyType, Value: var keyValue } 
                ? new InjectionKey(keyType, keyValue)
                : null;
            // Vanilla case: No KeyValuePair
            if (unwrappedItemType is INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceType)
            {
                var outwardFacingId = idRegister.GetOutwardFacingTypeId(interfaceType);
                var caseChoices = edgeContext.ScopeNode.CheckTypeProperties.MapToImplementations(interfaceType, injectionKey)
                    .Select(i => idRegister.GetInitialCaseId(edgeContext.ScopeNode, interfaceType, i))
                    .OfType<IdRegister.CaseIdResponse.Success>()
                    .Select(s => new CaseChoiceContext.Single(outwardFacingId, s.NextCaseId))
                    .ToImmutableArray();

                var interfaceResult = new ConcreteEnumerableNodeData.Interface(currentType, maybeWrappedItemType, caseChoices,
                    edgeContext is { Key: not KeyContext.None1 } or { CaseChoice: not CaseChoiceContext.None2 });

                return interfaceResult;
            }

            var singlePlainItemResult = new ConcreteEnumerableNodeData.SinglePlainItem(currentType, maybeWrappedItemType,
                edgeContext is { Key: not KeyContext.None1 } or { CaseChoice: not CaseChoiceContext.None2 });

            return singlePlainItemResult;
        }
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