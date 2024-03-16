using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IScopeCallNode : IScopeCallNodeBase
{
    string? DisposableCollectionReference { get; }
}

internal sealed partial class ScopeCallNode : ScopeCallNodeBase, IScopeCallNode
{
    private readonly IRangeNode _callingRange;

    public ScopeCallNode(
        // parameters
        ITypeSymbol callSideType,
        (string ContainerParameter, string TransientScopeInterfaceParameter) stringParams,
        IScopeNode scope,
        IRangeNode callingRange,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters, 
        IReadOnlyList<ITypeSymbol> typeParameters,
        IFunctionCallNode? initialization,
        ScopeCallNodeOuterMapperParam outerMapperParam,
        
        // dependencies
        IFunctionNode calledFunction, 
        IReferenceGenerator referenceGenerator) 
        : base(
            callSideType,
            scope,
            parameters,
            typeParameters,
            initialization,
            outerMapperParam,
            calledFunction,
            referenceGenerator)
    {
        _callingRange = callingRange;
        AdditionalPropertiesForConstruction = 
        [
            (scope.ContainerReference ?? "", stringParams.ContainerParameter), 
            (scope.TransientScopeInterfaceReference, stringParams.TransientScopeInterfaceParameter)
        ];
        callingRange.DisposalHandling.RegisterSyncDisposal();
    }

    public string? DisposableCollectionReference => DisposalType switch
    {
        Configuration.DisposalType.Async | Configuration.DisposalType.Sync or Configuration.DisposalType.Async => 
            _callingRange.DisposalHandling.AsyncCollectionReference,
        Configuration.DisposalType.Sync => _callingRange.DisposalHandling.SyncCollectionReference,
        _ => null
    };

    protected override (string Name, string Reference)[] AdditionalPropertiesForConstruction { get; }
}