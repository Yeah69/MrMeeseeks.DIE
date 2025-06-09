using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph;

internal record ResolutionStep(TypeNode Current, EdgeContext Context, Location CurrentResolvedLocation);

internal interface IInjectionGraphBuilder
{
    IReadOnlyList<ITypeNodeFunction> Functions { get; }
    void BuildForRootType(
        ITypeSymbol type, 
        string entryFunctionName, 
        IReadOnlyList<ITypeSymbol> overrides,
        Location createFunctionAttributeLocation);
    void AssignFunctions();
}

internal class InjectionGraphBuilder(
    InjectionGraphBuilderResolutionSteps resolutionSteps,
    TypeNodeManager typeNodeManager,
    ConcreteEntryFunctionNodeManager concreteEntryFunctionNodeManager,
    OverrideContextManager overrideContextManager,
    Func<TypeNode, Accessibility?, TypeNodeFunction> functionFactory,
    Func<ITypeNodeFunction, FunctionEdgeType> functionEdgeTypeFactory,
    WellKnownTypesCollections wellKnownTypesCollections)
    : IInjectionGraphBuilder, IContainerInstance
{
    private readonly List<ConcreteEntryFunctionNode> _concreteEntryFunctionNodes = [];
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
            new DomainContext.Container(), 
            overrideContext, 
            new KeyContext.None(), 
            new InitialCaseChoiceContext.None());
        var concreteEntryFunctionNodeData = new ConcreteEntryFunctionNodeData(entryFunctionName, rootType, overrides);
        var concreteEntryFunctionNode = concreteEntryFunctionNodeManager.GetOrAddNode(concreteEntryFunctionNodeData);
        _concreteEntryFunctionNodes.Add(concreteEntryFunctionNode);
        var rootTypeNodes = concreteEntryFunctionNode.ConnectIfNotAlready(rootEdgeContext).Select(t => t.TypeNode).ToList();
        var queue = new Queue<ResolutionStep>();
        foreach (var rootTypeNode in rootTypeNodes)
            queue.Enqueue(new ResolutionStep(rootTypeNode, rootEdgeContext, createFunctionAttributeLocation));
        while (queue.Count > 0)
        {
            var (typeNode, edgeContext, currentResolvedLocation) = queue.Dequeue();
            MakeResolutionStep(typeNode, edgeContext, queue, currentResolvedLocation);
        }
    }

    private void MakeResolutionStep(
        TypeNode typeNode,
        EdgeContext edgeContext,
        Queue<ResolutionStep> queue,
        Location currentResolvedLocation)
    {
        if (typeNode.ContainsOutgoingEdgeFor(edgeContext))
            return;

        var typeNodeType = typeNode.Type;
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
            case INamedTypeSymbol { TypeArguments.Length: >= 1 } maybeFunctor when maybeFunctor.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal):
                resolutionSteps.FunctorStep(maybeFunctor, typeNode, edgeContext, queue, currentResolvedLocation);
                break;
            case INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceType:
                resolutionSteps.InterfaceStep(interfaceType, typeNode, edgeContext, queue, currentResolvedLocation);
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
        foreach (var typedInjectionNode in typeNodeManager.AllTypeNodes)
            if (// if multiple incoming edges x contexts
                typedInjectionNode.Incoming.SelectMany(i => i.Contexts).Count() > 1 
                // or incoming edge is from a concrete functor (Func, Lazy, ThreadLocal)
                || typedInjectionNode.Incoming.Any(e => e.Source is ConcreteFunctorNode)
                // or outgoing edges contain concrete enumerable
                || typedInjectionNode.Outgoing.Any(e => e.Target is ConcreteEnumerableNode))
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