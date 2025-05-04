using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryFunctionNode : IFactoryNodeBase
{
    IReadOnlyList<(string Name, IElementNode Element)> Parameters { get; }
}

internal sealed partial class FactoryFunctionNode : FactoryNodeBase, IFactoryFunctionNode
{
    private readonly IMethodSymbol _methodSymbol;
    private readonly IElementNodeMapperBase _elementNodeMapperBase;
    private readonly List<(string, IElementNode)> _parameters = [];

    internal FactoryFunctionNode(
        IMethodSymbol methodSymbol,
        IElementNodeMapperBase elementNodeMapperBase,
        
        IFunctionNode parentFunction,
        ITaskBasedQueue taskBasedQueue,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes) 
        : base(methodSymbol.ReturnType, methodSymbol, parentFunction, taskBasedQueue, referenceGenerator, wellKnownTypes)
    {
        _methodSymbol = methodSymbol;
        _elementNodeMapperBase = elementNodeMapperBase;
    }

    public override void Build(PassedContext passedContext)
    {
        _parameters.AddRange(_methodSymbol
            .Parameters
            .Select(p => (p.Name, _elementNodeMapperBase.Map(p.Type, passedContext))));
        base.Build(passedContext);
    }
    
    public IReadOnlyList<(string, IElementNode)> Parameters => _parameters;
}