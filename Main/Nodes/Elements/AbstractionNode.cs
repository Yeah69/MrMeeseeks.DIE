using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IAbstractionNode : IElementNode
{
    IElementNode Implementation { get; }
}

internal class AbstractionNode : IAbstractionNode
{
    private readonly INamedTypeSymbol _implementationType;
    private readonly IElementNodeMapperBase _mapper;

    internal AbstractionNode(
        INamedTypeSymbol abstractionType, 
        INamedTypeSymbol implementationType,
        IElementNodeMapperBase mapper,
        
        IReferenceGenerator referenceGenerator)
    {
        _implementationType = implementationType;
        _mapper = mapper;
        TypeFullName = abstractionType.FullName();
        Reference = referenceGenerator.Generate(abstractionType);
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        Implementation = _mapper.MapToImplementation(new(true, true), _implementationType, implementationStack);
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitAbstractionNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public IElementNode Implementation { get; private set; } = null!;
}