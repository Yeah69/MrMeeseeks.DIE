using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IScopeCallNode : IFunctionCallNode
{
    string ContainerParameter { get; }
    string TransientScopeInterfaceParameter { get; }
    string ScopeFullName { get; }
    string ScopeReference { get; }
    DisposalType DisposalType { get; }
    string? DisposableCollectionReference { get; }
}

internal class ScopeCallNode : FunctionCallNode, IScopeCallNode
{
    private readonly IScopeNode _scope;
    private readonly IRangeNode _callingRange;

    public override void Accept(INodeVisitor nodeVisitor)
    {
        nodeVisitor.VisitScopeCallNode(this);
    }

    public ScopeCallNode(
        string containerParameter, 
        string transientScopeInterfaceParameter,
        IScopeNode scope,
        IRangeNode callingRange,
        IFunctionNode calledFunction, 
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters, 
        IReferenceGenerator referenceGenerator) 
        : base(null, calledFunction, parameters, referenceGenerator)
    {
        _scope = scope;
        _callingRange = callingRange;
        ContainerParameter = containerParameter;
        TransientScopeInterfaceParameter = transientScopeInterfaceParameter;
        ScopeFullName = scope.FullName;
        ScopeReference = referenceGenerator.Generate("scopeRoot");
        callingRange.DisposalHandling.RegisterSyncDisposal();
    }

    public override string OwnerReference => ScopeReference;

    public string ContainerParameter { get; }
    public string TransientScopeInterfaceParameter { get; }
    public string ScopeFullName { get; }
    public string ScopeReference { get; }
    public DisposalType DisposalType => _scope.DisposalType;

    public string? DisposableCollectionReference => DisposalType switch
    {
        DisposalType.Async => _callingRange.DisposalHandling.AsyncCollectionReference,
        DisposalType.Sync => _callingRange.DisposalHandling.SyncCollectionReference,
        _ => null
    };
}