using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal abstract class EdgeBase<TSource, TTarget> 
    : IEdge
    where TSource : INode 
    where TTarget : INode
{
    private readonly List<EdgeContext> _contexts = [];

    protected EdgeBase(TSource source, TTarget target, EdgeRegistry registry)
    {
        Source = source;
        Target = target;
        registry.Register(this);
    }

    public TSource Source { get; }
    public TTarget Target { get; protected set; }
    public IReadOnlyList<EdgeContext> Contexts => _contexts;
    public INode SourceAsNode => Source;
    public INode TargetAsNode => Target;
    
    internal bool AddContext(EdgeContext context)
    {
        if (_contexts.Contains(context))
            return false;

        _contexts.Add(context);
        return true;
    }
}