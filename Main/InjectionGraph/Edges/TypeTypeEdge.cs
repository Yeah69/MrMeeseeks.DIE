using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal sealed class TypeTypeEdge : EdgeBase<TypeNode, TypeNode>
{
    public TypeTypeEdge((TypeNode source, TypeNode target) tuple, EdgeRegistry registry) 
        : base(tuple.source, tuple.target, registry) => 
        tuple.target.AddIncomingTypeTypeEdge(this);
}