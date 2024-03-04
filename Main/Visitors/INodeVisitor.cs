using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.Visitor;


namespace MrMeeseeks.DIE.Visitors;

[VisitorInterface(typeof(INode))]
internal partial interface INodeVisitor;