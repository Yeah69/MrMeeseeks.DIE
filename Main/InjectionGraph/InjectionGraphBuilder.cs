using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IInjectionGraphBuilder
{
    IReadOnlyList<IFunction> Functions { get; }
    void BuildForRootType(ITypeSymbol type, string entryFunctionName, IReadOnlyList<ITypeSymbol> overrides);
    void AssignFunctions();
}

internal class InjectionGraphBuilder(
    IContainerCheckTypeProperties containerCheckTypeProperties,
    IInjectablePropertyExtractor injectablePropertyExtractor,
    TypeNodeManager typeNodeManager,
    ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
    ConcreteImplementationNodeManager concreteImplementationNodeManager,
    ConcreteOverrideNodeManager concreteOverrideNodeManager,
    OverrideContextManager overrideContextManager,
    Func<TypeNode, Accessibility?, Function> functionFactory,
    Func<IFunction, FunctionEdgeType> functionEdgeTypeFactory,
    Func<TypeNode, IConcreteNode, ConcreteEdge> concreteEdgeFactory)
    : IInjectionGraphBuilder, IContainerInstance
{
    private readonly List<ConcreteEntryFunctionNode> _concreteEntryFunctionNodes = [];
    private readonly List<IFunction> _functions = [];

    public IReadOnlyList<IFunction> Functions => _functions;

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
                
                ConnectToTypeNodeIfNotAlready(concreteOverrideNode);
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Class } namedTypeSymbol:
                var implementation = containerCheckTypeProperties.MapToSingleFittingImplementation(namedTypeSymbol, null);
                if (implementation is null)
                    throw new ArgumentException("Type is not a fitting implementation", nameof(typeNodeType)); // ToDo: Better exception
                
                // Constructor
                var constructor = containerCheckTypeProperties.GetConstructorChoiceFor(implementation);
                if (constructor is null)
                    throw new ArgumentException("Type does not have a fitting constructor", nameof(typeNodeType)); // ToDo: Better exception
                
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
                
                ConnectToTypeNodeIfNotAlready(concreteImplementationNode);
                
                foreach (var node in concreteImplementationNode.ConnectIfNotAlready(edgeContext))
                    queue.Enqueue(new ResolutionStep(node, edgeContext));
                break;
            default:
                throw new ArgumentException("Type is unsupported for injection resolution", nameof(typeNodeType));
        }

        return;
        
        void ConnectToTypeNodeIfNotAlready(IConcreteNode concreteNode)
        {
            if (!typeNode.TryGetOutgoingEdgeFor(concreteNode, out var existingEdge))
            {
                existingEdge = concreteEdgeFactory(typeNode, concreteNode);
                typeNode.AddOutgoing(existingEdge);
            }

            existingEdge.AddContext(edgeContext);
        }
    }

    public void AssignFunctions()
    {
        foreach (var typedInjectionNode in typeNodeManager.AllTypeNodes)
        {
            if (typedInjectionNode.Incoming.Count > 1)
            {
                var function = functionFactory(typedInjectionNode, Accessibility.Internal);
                _functions.Add(function);
                foreach (var incoming in typedInjectionNode.Incoming)
                    incoming.Type = functionEdgeTypeFactory(function);
            }
        }

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
    }
}