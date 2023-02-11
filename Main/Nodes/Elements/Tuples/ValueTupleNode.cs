using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Tuples;

internal interface IValueTupleNode : IElementNode
{
    IReadOnlyList<(string Name, IElementNode Node)> Parameters { get; }
}

internal class ValueTupleNode : IValueTupleNode
{
    private readonly INamedTypeSymbol _valueTupleType;
    private readonly IElementNodeMapperBase _elementNodeMapper;
    private readonly List<(string Name, IElementNode Node)> _parameters = new();

    internal ValueTupleNode(
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
        var constructor = _valueTupleType
            .InstanceConstructors
            // Don't take the parameterless (struct)-constructor
            .First(c => c.Parameters.Length > 0);
        _parameters.AddRange(constructor
            .Parameters
            .Select(p => (p.Name, _elementNodeMapper.Map(p.Type, implementationStack))));
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitValueTupleNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IReadOnlyList<(string Name, IElementNode Node)> Parameters => _parameters;
}