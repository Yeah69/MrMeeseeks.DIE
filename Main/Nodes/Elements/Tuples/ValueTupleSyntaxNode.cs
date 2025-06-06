using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.Tuples;

internal interface IValueTupleSyntaxNode : IElementNode
{
    IReadOnlyList<IElementNode> Items { get; }
}

internal sealed partial class ValueTupleSyntaxNode : IValueTupleSyntaxNode
{
    private readonly INamedTypeSymbol _valueTupleType;
    private readonly IElementNodeMapperBase _elementNodeMapper;
    private List<IElementNode> _items = [];

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
    
    public void Build(PassedContext passedContext)
    {
        _items = GetTypeArguments(_valueTupleType).Select(type => _elementNodeMapper.Map(type, passedContext)).ToList();

        static IEnumerable<ITypeSymbol> GetTypeArguments(INamedTypeSymbol valueTupleType)
        {
            foreach (var typeArgument in valueTupleType.TypeArguments)
            {
                if (typeArgument.FullName().StartsWith("(", StringComparison.Ordinal) && typeArgument.FullName().EndsWith(")", StringComparison.Ordinal) &&
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

    public string TypeFullName { get; }
    public string Reference { get; }
    public IReadOnlyList<IElementNode> Items => _items;
}