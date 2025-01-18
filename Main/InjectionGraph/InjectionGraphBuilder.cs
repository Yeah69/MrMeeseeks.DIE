using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IInjectionGraphBuilder
{
    IReadOnlyList<ITypeNodeFunction> Functions { get; }
    void BuildForRootType(ITypeSymbol type, string entryFunctionName, IReadOnlyList<ITypeSymbol> overrides);
    void AssignFunctions();
}

internal class InjectionGraphBuilder(
    IContainerCheckTypeProperties containerCheckTypeProperties,
    ILocalDiagLogger containerDiagLogger,
    IInjectablePropertyExtractor injectablePropertyExtractor,
    TypeNodeManager typeNodeManager,
    ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
    ConcreteImplementationNodeManager concreteImplementationNodeManager,
    ConcreteFunctorNodeManager concreteFunctorNodeManager,
    ConcreteOverrideNodeManager concreteOverrideNodeManager,
    OverrideContextManager overrideContextManager,
    Lazy<ConcreteExceptionNode> concreteExceptionNode,
    Func<TypeNode, Accessibility?, TypeNodeFunction> functionFactory,
    Func<ITypeNodeFunction, FunctionEdgeType> functionEdgeTypeFactory,
    Func<TypeNode, IConcreteNode, ConcreteEdge> concreteEdgeFactory,
    WellKnownTypes wellKnownTypes)
    : IInjectionGraphBuilder, IContainerInstance
{
    private readonly List<ConcreteEntryFunctionNode> _concreteEntryFunctionNodes = [];
    private readonly List<ITypeNodeFunction> _functions = [];

    public IReadOnlyList<ITypeNodeFunction> Functions => _functions;

    private record ResolutionStep(TypeNode Current, EdgeContext Context);

    public void BuildForRootType(ITypeSymbol rootType, string entryFunctionName, IReadOnlyList<ITypeSymbol> overrides)
    {
        var overrideContext = overrideContextManager.GetOrAddContext(overrides);
        var rootEdgeContext = new EdgeContext(new DomainContext.Container(), overrideContext, new KeyContext.None());
        var concreteEntryFunctionNodeData = new ConcreteEntryFunctionNodeData(entryFunctionName, rootType, overrides);
        var concreteEntryFunctionNode = concreteEntryFunctionNodeManager.GetOrAddNode(concreteEntryFunctionNodeData);
        _concreteEntryFunctionNodes.Add(concreteEntryFunctionNode);
        var rootTypeNodes = concreteEntryFunctionNode.ConnectIfNotAlready(rootEdgeContext);
        var queue = new Queue<ResolutionStep>();
        foreach (var rootTypeNode in rootTypeNodes)
            queue.Enqueue(new ResolutionStep(rootTypeNode, rootEdgeContext));
        while (queue.Count > 0)
        {
            var (typeNode, edgeContext) = queue.Dequeue();
            MakeResolutionStep(typeNode, edgeContext, queue);
        }
    }

    private void MakeResolutionStep(
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue)
    {
        if (typeNode.ContainsOutgoingEdgeFor(edgeContext))
            return;

        var typeNodeType = typeNode.Type;
        switch (typeNodeType)
        {
            case not null when edgeContext.Override is OverrideContext.Any any && any.Overrides.Contains(typeNodeType, CustomSymbolEqualityComparer.IncludeNullability):
                var concreteOverrideNodeData = new ConcreteOverrideNodeData(typeNodeType);
                var concreteOverrideNode = concreteOverrideNodeManager.GetOrAddNode(concreteOverrideNodeData);
                ConnectToTypeNodeIfNotAlready(concreteOverrideNode, edgeContext);
                break;
            case INamedTypeSymbol { TypeArguments.Length: >= 1 } maybeFunctor when 
                CustomSymbolEqualityComparer.Default.Equals(maybeFunctor.OriginalDefinition, wellKnownTypes.Lazy1)
                || CustomSymbolEqualityComparer.Default.Equals(maybeFunctor.OriginalDefinition, wellKnownTypes.ThreadLocal1)
                || maybeFunctor.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal):
                var concreteFunctorNodeData = new ConcreteFunctorNodeData(maybeFunctor);
                var concreteFunctorNode = concreteFunctorNodeManager.GetOrAddNode(concreteFunctorNodeData);
                var newOverrideContext = overrideContextManager.GetOrAddContext(concreteFunctorNode.FunctorParameterTypes);
                var newEdgeContext = edgeContext with { Override = newOverrideContext };
                ConnectToTypeNodeIfNotAlready(concreteFunctorNode, newEdgeContext);
                foreach (var node in concreteFunctorNode.ConnectIfNotAlready(newEdgeContext))
                    queue.Enqueue(new ResolutionStep(node, newEdgeContext));
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } namedTypeSymbol:
                var implementationResult = containerCheckTypeProperties.MapToSingleFittingImplementation(namedTypeSymbol, null);
                if (implementationResult is not ImplementationResult.Single { Implementation: { } implementation })
                {
                    ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext);
                    var logMessage = implementationResult switch
                    {
                        ImplementationResult.None => $"Class: No implementation registered for \"{namedTypeSymbol.FullName()}\".",
                        ImplementationResult.Multiple { Implementations: var implementations} => $"Class: Multiple implementations registered for \"{namedTypeSymbol.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                        _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
                    };
                    containerDiagLogger.Error(
                        ErrorLogData.ResolutionException(
                            logMessage,
                            namedTypeSymbol,
                            ImmutableStack<INamedTypeSymbol>.Empty), 
                        Location.None);
                    break;
                }
                
                // Constructor
                var constructorResult = containerCheckTypeProperties.GetConstructorChoiceFor(implementation);
                if (constructorResult is not ConstructorResult.Single { Constructor: {} constructor})
                {
                    ConnectToTypeNodeIfNotAlready(concreteExceptionNode.Value, edgeContext);
                    var logMessage = constructorResult switch
                    {
                        ConstructorResult.None => $"Class.Constructor: No visible constructor found for implementation {namedTypeSymbol.FullName()}",
                        ConstructorResult.Multiple => $"Class.Constructor: More than one visible constructor found for implementation {namedTypeSymbol.FullName()}",
                        _ => throw new InvalidOperationException("Unexpected ConstructorResult")
                    };
                    containerDiagLogger.Error(
                        ErrorLogData.ResolutionException(
                            logMessage,
                            namedTypeSymbol,
                            ImmutableStack<INamedTypeSymbol>.Empty), 
                        Location.None);
                    break;
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
                    properties = Array.Empty<IPropertySymbol>();
                
                var concreteImplementationNodeData = new ConcreteImplementationNodeData(
                    implementation,
                    constructor,
                    properties.OrderBy(p => p.Name).ToList());
                
                var concreteImplementationNode = concreteImplementationNodeManager.GetOrAddNode(concreteImplementationNodeData);
                
                ConnectToTypeNodeIfNotAlready(concreteImplementationNode, edgeContext);
                
                foreach (var node in concreteImplementationNode.ConnectIfNotAlready(edgeContext))
                    queue.Enqueue(new ResolutionStep(node, edgeContext));
                break;
            default:
                throw new ArgumentException("Type is unsupported for injection resolution", nameof(typeNodeType));
        }

        return;
        
        void ConnectToTypeNodeIfNotAlready(IConcreteNode concreteNode, EdgeContext edgeContextToContinueWith)
        {
            if (!typeNode.TryGetOutgoingEdgeFor(concreteNode, out var existingEdge))
            {
                existingEdge = concreteEdgeFactory(typeNode, concreteNode);
                typeNode.AddOutgoing(existingEdge);
            }

            existingEdge.AddContext(edgeContextToContinueWith);
        }
    }

    public void AssignFunctions()
    {
        foreach (var typedInjectionNode in typeNodeManager.AllTypeNodes)
            if (// if multiple incoming edges
                typedInjectionNode.Incoming.Count > 1 
                // or incoming edge is from a concrete functor (Func, Lazy, ThreadLocal)
                || typedInjectionNode.Incoming.Select(e => e.Source).Any(n => n is ConcreteFunctorNode))
                NewFunctionIfNotAlready(typedInjectionNode);

        foreach (var concreteEntryFunctionNode in _concreteEntryFunctionNodes)
        {
            if (concreteEntryFunctionNode.ReturnType is
                {
                    Target: { } rootTypeNode, 
                    Type: DefaultEdgeType
                } edge)
            {
                var function = functionFactory(rootTypeNode, Accessibility.Private);
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
                    function = functionFactory(typedInjectionNode, Accessibility.Internal);
                    _functions.Add(function);
                }
                foreach (var incoming in typedInjectionNode.Incoming)
                    incoming.Type = functionEdgeTypeFactory(function);
            }
        }
    }
}