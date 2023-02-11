using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Tuples;

internal interface IValueTupleSyntaxNode : IElementNode
{
    IReadOnlyList<IElementNode> Items { get; }
}

internal class ValueTupleSyntaxNode : IValueTupleSyntaxNode
{
    private readonly INamedTypeSymbol _valueTupleType;
    private readonly IElementNodeMapperBase _elementNodeMapper;
    private List<IElementNode> _items = new();

    internal ValueTupleSyntaxNode(
        INamedTypeSymbol valueTupleType,
        IElementNodeMapperBase elementNodeMapper,
        IReferenceGenerator referenceGenerator)
    {
        _valueTupleType = valueTupleType;
        _elementNodeMapper = elementNodeMapper;
        TypeFullName = valueTupleType.FullName();
        Reference = referenceGenerator.Generate(_valueTupleType);
    }
    
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        _items = GetTypeArguments(_valueTupleType).Select(type => _elementNodeMapper.Map(type, implementationStack)).ToList();

        static IEnumerable<ITypeSymbol> GetTypeArguments(INamedTypeSymbol valueTupleType)
        {
            foreach (var typeArgument in valueTupleType.TypeArguments)
            {
                if (typeArgument.FullName().StartsWith("(") && typeArgument.FullName().EndsWith(")") &&
                    typeArgument is INamedTypeSymbol nextSyntaxValueTupleType)
                {
                    foreach (var typeSymbol in GetTypeArguments(nextSyntaxValueTupleType))
                    {
                        yield return typeSymbol;
                    }
                }
                else
                {
                    yield return typeArgument;
                }
            }
        }
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitValueTupleSyntaxNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IReadOnlyList<IElementNode> Items => _items;
}