namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal interface IEdgeType;

internal sealed class DefaultEdgeType : IEdgeType
{
    internal static DefaultEdgeType Instance { get; } = new();
    private DefaultEdgeType() {}
}

internal sealed class FunctionEdgeType(ITypeNodeFunction function) : IEdgeType
{
    public ITypeNodeFunction Function { get; } = function;
}