using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface INullNode : IElementNode
{
    
}

internal class NullNode : INullNode
{
    internal NullNode(
        ITypeSymbol nullableType,
        
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = nullableType.FullName();
        Reference = referenceGenerator.Generate(nullableType);
    }
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitNullNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
}