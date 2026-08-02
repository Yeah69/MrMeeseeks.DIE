using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal abstract class ConcreteEdge(TypeNode source, IConcreteNode target, EdgeRegistry registry)
    : EdgeBase<TypeNode, IConcreteNode>(source, target, registry);

internal sealed class ConcreteSyncEdge(TypeNode source, IConcreteNode target, EdgeRegistry registry)
    : ConcreteEdge(source, target, registry);

internal sealed class ConcreteAsyncEdge(TypeNode source, IConcreteNode target, EdgeRegistry registry)
    : ConcreteEdge(source, target, registry);