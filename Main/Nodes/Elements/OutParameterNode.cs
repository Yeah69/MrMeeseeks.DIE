using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IOutParameterNode : IElementNode
{
}

internal class OutParameterNode : IOutParameterNode
{
    internal OutParameterNode(
        ITypeSymbol type,
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = type.FullName();
        Reference = referenceGenerator.Generate(type);
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitOutParameterNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
}