using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceFunctionGroupNode : IRangedInstanceFunctionGroupNodeBase
{
    IEnumerable<IRangedInstanceFunctionNode> Overloads { get; }
}

internal partial class RangedInstanceFunctionGroupNode : RangedInstanceFunctionGroupNodeBase, IRangedInstanceFunctionGroupNode
{
    private readonly INamedTypeSymbol _type;
    private readonly IContainerNode _parentContainer;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangedInstanceFunctionNodeRoot> _rangedInstanceFunctionNodeFactory;
    private readonly List<IRangedInstanceFunctionNode> _overloads = new();

    internal RangedInstanceFunctionGroupNode(
        // parameters
        ScopeLevel level,
        INamedTypeSymbol type,
        
        // dependencies
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator,
        Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangedInstanceFunctionNodeRoot> rangedInstanceFunctionNodeFactory)
        : base(level, type, referenceGenerator)
    {
        _type = type;
        _parentContainer = parentContainer;
        _rangedInstanceFunctionNodeFactory = rangedInstanceFunctionNodeFactory;
    }

    public IEnumerable<IRangedInstanceFunctionNode> Overloads => _overloads;
    
    public override IRangedInstanceFunctionNode BuildFunction(IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunction(
            callingFunction,
            _overloads,
            () => _rangedInstanceFunctionNodeFactory(
                Level,
                _type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, new(ImmutableStack<INamedTypeSymbol>.Empty, null)));
}