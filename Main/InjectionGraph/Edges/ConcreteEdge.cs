using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal sealed class ConcreteEdge(TypeNode source, IConcreteNode target, EdgeRegistry registry)
    : EdgeBase<TypeNode, IConcreteNode>(source, target, registry);