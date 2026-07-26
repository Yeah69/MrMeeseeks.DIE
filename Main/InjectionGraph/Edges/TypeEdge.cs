using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal sealed class TypeEdge : EdgeBase<IConcreteNode, TypeNode>
{
    public TypeEdge(IConcreteNode source, TypeNode target, EdgeRegistry registry) : base(source, target, registry)
    {
        target.AddIncoming(this);
        Type = DefaultEdgeType.Instance;
    }

    internal IEdgeType Type { get; set; }
    
    internal void ReplaceTarget(TypeNode newTarget) => 
        Target = newTarget;
}