using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IErrorNode : IElementNode
{
    string Message { get; }
}

internal class ErrorNode : IErrorNode
{
    internal ErrorNode(
        string message)
    {
        Message = message;
    }
    
    public void Build()
    {
        throw new NotImplementedException();
    }

    public void Accept(INodeVisitor nodeVisitor)
    {
        throw new NotImplementedException();
    }

    public string Message { get; }
    public string TypeFullName { get; } = "Errors have no type";
    public string Reference { get; } = "Errors have no reference";
}