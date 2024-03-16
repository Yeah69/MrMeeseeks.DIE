namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IReferenceNode : IElementNode;

internal sealed partial class ReferenceNode : IReferenceNode
{
    internal ReferenceNode(string reference) => Reference = reference;

    public void Build(PassedContext passedContext) { }

    public string TypeFullName => throw new InvalidOperationException("TypeFullName from ReferenceNode should not be used");
    public string Reference { get; }
}