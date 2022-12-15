namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IElementNode : INode
{
    string TypeFullName { get; }
    string Reference { get; }
}