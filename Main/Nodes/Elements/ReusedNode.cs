namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IReusedNode : IElementNode
{
    IElementNode Inner { get; }
}

internal partial class ReusedNode : IReusedNode
{
    internal ReusedNode(
        IElementNode innerNode) =>
        Inner = innerNode;

    public void Build(PassedContext passedContext) { }

    public string TypeFullName => Inner.TypeFullName;
    public string Reference => Inner.Reference;
    public IElementNode Inner { get; }
}