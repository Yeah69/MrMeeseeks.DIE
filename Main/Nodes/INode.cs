using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes;

internal interface INode
{
    void Build();
    void Accept(INodeVisitor nodeVisitor);
}