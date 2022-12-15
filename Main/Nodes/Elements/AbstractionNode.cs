using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IAbstractionNode : IElementNode
{
    IElementNode Implementation { get; }
}

internal class AbstractionNode : IAbstractionNode
{
    internal AbstractionNode(
        INamedTypeSymbol abstractionType, 
        IElementNode implementation,
        IReferenceGenerator referenceGenerator)
    {
        Implementation = implementation;
        TypeFullName = abstractionType.FullName();
        Reference = referenceGenerator.Generate(abstractionType);
    }

    public void Build()
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitAbstractionNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IElementNode Implementation { get; }
}