namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface IEdgeType;

internal class DefaultEdgeType : IEdgeType
{
    internal static DefaultEdgeType Instance { get; } = new();
    private DefaultEdgeType() {}
}

internal class FunctionEdgeType(IFunction function) : IEdgeType
{
    public IFunction Function { get; } = function;
}