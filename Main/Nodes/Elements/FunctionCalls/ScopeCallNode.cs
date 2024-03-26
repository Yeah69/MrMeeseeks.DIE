using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IScopeCallNode : IScopeCallNodeBase
{
    string? DisposableCollectionReference { get; }
    string SubDisposalReference { get; }
}

internal sealed partial class ScopeCallNode : ScopeCallNodeBase, IScopeCallNode
{
    private readonly IRangeNode _callingRange;

    internal record struct Params(
        ITypeSymbol CallSideType, 
        string ContainerParameter,
        string TransientScopeInterfaceParameter,
        IScopeNode Scope,
        IRangeNode CallingRange,
        IFunctionNode CallingFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> Parameters,
        IReadOnlyList<ITypeSymbol> TypeParameters,
        IFunctionCallNode? Initialization,
        ScopeCallNodeOuterMapperParam OuterMapperParam);
    internal ScopeCallNode(
        // parameters
        Params parameters,
        
        // dependencies
        IFunctionNode calledFunction, 
        IReferenceGenerator referenceGenerator) 
        : base(
            parameters.CallSideType,
            parameters.Scope,
            parameters.Parameters,
            parameters.TypeParameters,
            parameters.Initialization,
            parameters.OuterMapperParam,
            calledFunction,
            referenceGenerator)
    {
        _callingRange = parameters.CallingRange;
        AdditionalPropertiesForConstruction = 
        [
            (parameters.Scope.ContainerReference ?? "", parameters.ContainerParameter), 
            (parameters.Scope.TransientScopeInterfaceReference, parameters.TransientScopeInterfaceParameter)
        ];
        parameters.CallingRange.DisposalHandling.RegisterSyncDisposal();
        SubDisposalReference = parameters.CallingFunction.SubDisposalNode.Reference;
    }

    public string? DisposableCollectionReference => DisposalType switch
    {
        Configuration.DisposalType.Async | Configuration.DisposalType.Sync or Configuration.DisposalType.Async => 
            _callingRange.DisposalHandling.AsyncCollectionReference,
        Configuration.DisposalType.Sync => _callingRange.DisposalHandling.SyncCollectionReference,
        _ => null
    };

    public string SubDisposalReference { get; }

    protected override (string Name, string Reference)[] AdditionalPropertiesForConstruction { get; }
}