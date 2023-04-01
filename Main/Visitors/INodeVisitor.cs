using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.Visitor;

[assembly:VisitorInterfacePair(typeof(INodeVisitor), typeof(INode))]

namespace MrMeeseeks.DIE.Visitors;

internal partial interface INodeVisitor
{
}