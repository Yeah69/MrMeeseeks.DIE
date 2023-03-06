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
    private readonly IRangeNode _parentRange;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IUserDefinedElementsBase _userDefinedElementsBase;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IUserDefinedElementsBase, ICheckTypeProperties, IRangedInstanceFunctionNodeRoot> _rangedInstanceFunctionNodeFactory;
    private readonly List<IRangedInstanceFunctionNode> _overloads = new();

    internal RangedInstanceFunctionGroupNode(
        ScopeLevel level,
        INamedTypeSymbol type,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        ICheckTypeProperties checkTypeProperties,
        IUserDefinedElementsBase userDefinedElements,
        IReferenceGenerator referenceGenerator,
        
        Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IUserDefinedElementsBase, ICheckTypeProperties, IRangedInstanceFunctionNodeRoot> rangedInstanceFunctionNodeFactory)
        : base(level, type, referenceGenerator)
    {
        _type = type;
        _parentContainer = parentContainer;
        _parentRange = parentRange;
        _checkTypeProperties = checkTypeProperties;
        _userDefinedElementsBase = userDefinedElements;
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
                _userDefinedElementsBase,
                _checkTypeProperties)
                .Function
                .EnqueueBuildJobTo(_parentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty));
}