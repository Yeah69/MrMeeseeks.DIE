using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface ITransientScopeCallNode : IFunctionCallNode
{
    string ContainerParameter { get; }
    string? ContainerReference { get; }
    string TransientScopeFullName { get; }
    string TransientScopeReference { get; }
    DisposalType DisposalType { get; }
    string TransientScopeDisposalReference { get; }
}

internal class TransientScopeCallNode : FunctionCallNode, ITransientScopeCallNode
{
    private readonly ITransientScopeNode _scope;

    public override void Accept(INodeVisitor nodeVisitor)
    {
        nodeVisitor.VisitTransientScopeCallNode(this);
    }

    public TransientScopeCallNode(
        string containerParameter, 
        ITransientScopeNode scope,
        IContainerNode parentContainer,
        IRangeNode callingRange,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters, 
        IReferenceGenerator referenceGenerator) 
        : base(null, calledFunction, parameters, referenceGenerator)
    {
        _scope = scope;
        ContainerParameter = containerParameter;
        TransientScopeFullName = scope.FullName;
        TransientScopeReference = referenceGenerator.Generate("transientScopeRoot");
        ContainerReference = callingRange.ContainerReference;
        TransientScopeDisposalReference = parentContainer.TransientScopeDisposalReference;
    }

    public override string OwnerReference => TransientScopeReference;

    public string ContainerParameter { get; }
    public string? ContainerReference { get; }
    public string TransientScopeFullName { get; }
    public string TransientScopeReference { get; }
    public DisposalType DisposalType => _scope.DisposalType;
    public string TransientScopeDisposalReference { get; }
}