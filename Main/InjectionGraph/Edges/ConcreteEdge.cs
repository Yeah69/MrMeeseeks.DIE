using MrMeeseeks.DIE.InjectionGraph.Edges;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal class ConcreteEdge(TypeNode source, IConcreteNode target)
{
    private readonly List<EdgeContext> _contexts = [];
    internal TypeNode Source { get; } = source;
    internal IConcreteNode Target { get; } = target;
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