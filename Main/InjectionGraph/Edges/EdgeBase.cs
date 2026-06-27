using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph.Edges;

internal abstract class EdgeBase<TSource, TTarget> 
    : IEdge
    where TSource : INode 
    where TTarget : INode
{
    private readonly List<EdgeContext> _contexts = [];
    private readonly List<EdgeContext> _asyncContexts = [];

    protected EdgeBase(TSource source, TTarget target, EdgeRegistry registry)
    {
        Source = source;
        Target = target;
        registry.Register(this);
    }

    public TSource Source { get; }
    public TTarget Target { get; }
    public IReadOnlyList<EdgeContext> Contexts => _contexts;
    public IReadOnlyList<EdgeContext> AsyncContexts => _asyncContexts;
    public INode SourceAsNode => Source;
    public INode TargetAsNode => Target;
    
    public void AsyncAdjust(HashSet<int> asyncResolutionIds)
    {
        var asyncContexts = _contexts.Where(c => asyncResolutionIds.Contains(c.ResolutionId)).ToList();
        _asyncContexts.AddRange(asyncContexts);
        foreach (var asyncContext in asyncContexts)
            _contexts.Remove(asyncContext);
    }
    
    internal bool AddContext(EdgeContext context)
    {
        if (_contexts.Contains(context))
            return false;

        _contexts.Add(context);
        return true;
    }
}