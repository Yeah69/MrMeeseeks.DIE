namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface IEdgeType;

internal class DefaultEdgeType : IEdgeType
{
    internal static DefaultEdgeType Instance { get; } = new();
    private DefaultEdgeType() {}
}

internal class FunctionEdgeType(ITypeNodeFunction function) : IEdgeType
{
    public ITypeNodeFunction Function { get; } = function;
}