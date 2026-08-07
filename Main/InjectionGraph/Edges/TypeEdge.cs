using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal sealed class TypeEdge : EdgeBase<IConcreteNode, TypeNode>
{
    public TypeEdge(IConcreteNode source, TypeNode target, EdgeRegistry registry) 
        : base(source, target, registry) => 
        target.AddIncomingTypeEdge(this);
}