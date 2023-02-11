using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface ICollectionNode : IElementNode
{
    IReadOnlyList<IElementNode> Items { get; }
    string ItemTypeFullName { get; }
}

internal class CollectionNode : ICollectionNode
{
    internal CollectionNode(
        ITypeSymbol collectionType,
        IReadOnlyList<IElementNode> itemNodes,
        IReferenceGenerator referenceGenerator)
    {
        Items = itemNodes;
        TypeFullName = collectionType.FullName();
        ItemTypeFullName = CollectionUtility.GetCollectionsInnerType(collectionType).FullName();
        Reference = referenceGenerator.Generate(collectionType);
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitCollectionNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IReadOnlyList<IElementNode> Items { get; }
    public string ItemTypeFullName { get; }
}