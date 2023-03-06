using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceFunctionGroupNode : IRangedInstanceFunctionGroupNodeBase
{
    IEnumerable<IRangedInstanceFunctionNode> Overloads { get; }
}

internal class RangedInstanceFunctionGroupNode : RangedInstanceFunctionGroupNodeBase, IRangedInstanceFunctionGroupNode
{
    private readonly INamedTypeSymbol _type;
    private readonly IContainerNode _parentContainer;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICheckTypeProperties, IRangedInstanceFunctionNodeRoot> _rangedInstanceFunctionNodeFactory;
    private readonly List<IRangedInstanceFunctionNode> _overloads = new();

    internal RangedInstanceFunctionGroupNode(
        ScopeLevel level,
        INamedTypeSymbol type,
        IContainerNode parentContainer,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        
        Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICheckTypeProperties, IRangedInstanceFunctionNodeRoot> rangedInstanceFunctionNodeFactory)
        : base(level, type, referenceGenerator)
    {
        _type = type;
        _parentContainer = parentContainer;
        _checkTypeProperties = checkTypeProperties;
        _rangedInstanceFunctionNodeFactory = rangedInstanceFunctionNodeFactory;
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitRangedInstanceFunctionGroupNode(this);
    
    public IEnumerable<IRangedInstanceFunctionNode> Overloads => _overloads;
    
    public override IRangedInstanceFunctionNode BuildFunction(IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunction(
            callingFunction,
            _overloads,
            () => _rangedInstanceFunctionNodeFactory(
                Level,
                _type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList(),
                _checkTypeProperties)
                .Function
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty));
}