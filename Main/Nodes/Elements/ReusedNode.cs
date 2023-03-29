using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IReusedNode : IElementNode
{
    IElementNode Inner { get; }
}

internal class ReusedNode : IReusedNode
{
    internal ReusedNode(
        IElementNode innerNode) =>
        Inner = innerNode;

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        
    }

    public void Accept(INodeVisitor nodeVisitor) => 
        nodeVisitor.VisitReusedNode(this);

    public string TypeFullName => Inner.TypeFullName;
    public string Reference => Inner.Reference;
    public IElementNode Inner { get; }
}