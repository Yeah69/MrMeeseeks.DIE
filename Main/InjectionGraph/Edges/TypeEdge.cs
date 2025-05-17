using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal class TypeEdge
{
    private readonly List<EdgeContext> _contexts = [];

    public TypeEdge(IConcreteNode source, TypeNode target)
    {
        Source = source;
        Target = target;
        target.AddIncoming(this);
        Type = DefaultEdgeType.Instance;
    }

    internal IConcreteNode Source { get; }
    internal TypeNode Target { get; }
    internal IEdgeType Type { get; set; }
    internal IReadOnlyList<EdgeContext> Contexts => _contexts;
    
    internal bool AddContext(EdgeContext context)
    {
        if (!_contexts.Contains(context))
        {
            _contexts.Add(context);
            return true;
        }

        return false;
    }
    
}