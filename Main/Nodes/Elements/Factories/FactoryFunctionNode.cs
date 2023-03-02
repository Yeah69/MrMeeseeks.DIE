using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryFunctionNode : IFactoryNodeBase
{
    IReadOnlyList<(string Name, IElementNode Element)> Parameters { get; }
}

internal class FactoryFunctionNode : FactoryNodeBase, IFactoryFunctionNode
{
    private readonly IMethodSymbol _methodSymbol;
    private readonly IElementNodeMapperBase _elementNodeMapperBase;
    private readonly List<(string, IElementNode)> _parameters = new ();

    internal FactoryFunctionNode(
        IMethodSymbol methodSymbol,
        IFunctionNode parentFunction,
        IElementNodeMapperBase elementNodeMapperBase,
        
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext) 
        : base(methodSymbol.ReturnType, methodSymbol, parentFunction, referenceGenerator, containerWideContext)
    {
        _methodSymbol = methodSymbol;
        _elementNodeMapperBase = elementNodeMapperBase;
    }

    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        _parameters.AddRange(_methodSymbol
            .Parameters
            .Select(p => (p.Name, _elementNodeMapperBase.Map(p.Type, implementationStack))));
        base.Build(implementationStack);
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitFactoryFunctionNode(this);
    
    public IReadOnlyList<(string, IElementNode)> Parameters => _parameters;
}