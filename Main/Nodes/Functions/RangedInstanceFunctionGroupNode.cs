using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Ranges;
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
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionNode> _rangedInstanceFunctionNodeFactory;
    private readonly List<IRangedInstanceFunctionNode> _overloads = new();

    internal RangedInstanceFunctionGroupNode(
        ScopeLevel level,
        INamedTypeSymbol type,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        ICheckTypeProperties checkTypeProperties,
        IUserDefinedElements userDefinedElements,
        IReferenceGenerator referenceGenerator,
        
        Func<ScopeLevel, INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionNode> rangedInstanceFunctionNodeFactory)
        : base(level, type, referenceGenerator)
    {
        _type = type;
        _parentContainer = parentContainer;
        _parentRange = parentRange;
        _checkTypeProperties = checkTypeProperties;
        _userDefinedElements = userDefinedElements;
        _referenceGenerator = referenceGenerator;
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
                callingFunction.Overrides.Select(kvp => kvp.Value.Item1).ToList(),
                _parentRange,
                _parentContainer,
                _userDefinedElements,
                _checkTypeProperties,
                _referenceGenerator).EnqueueTo(_parentContainer.BuildQueue));
}