using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IParameterNode : IElementNode
{
    ITypeSymbol Type { get; }
}

internal class ParameterNode : IParameterNode
{
    internal ParameterNode(ITypeSymbol type, IReferenceGenerator referenceGenerator)
    {
        Type = type;
        TypeFullName = type.FullName();
        Reference = referenceGenerator.Generate(Type);
    }

    public void Build()
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitParameterNode(this);

    public string TypeFullName { get; }
    public string Reference { get; private set; } = "";
    public ITypeSymbol Type { get; }
}