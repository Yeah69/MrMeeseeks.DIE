using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface ITransientScopeCallNode : IFunctionCallNode
{
    string ContainerParameter { get; }
    string? ContainerReference { get; }
    string TransientScopeFullName { get; }
    string TransientScopeReference { get; }
    DisposalType DisposalType { get; }
    string TransientScopeDisposalReference { get; }
    IFunctionCallNode? Initialization { get; }
}

internal partial class TransientScopeCallNode(string containerParameter,
        ITransientScopeNode scope,
        IContainerNode parentContainer,
        IRangeNode callingRange,
        IFunctionNode calledFunction,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IFunctionCallNode? initialization,
        IReferenceGenerator referenceGenerator)
    : FunctionCallNode(null, calledFunction, parameters, referenceGenerator), ITransientScopeCallNode
{
    public override string OwnerReference => TransientScopeReference;

    public string ContainerParameter { get; } = containerParameter;
    public string? ContainerReference { get; } = callingRange.ContainerReference;
    public string TransientScopeFullName { get; } = scope.FullName;
    public string TransientScopeReference { get; } = referenceGenerator.Generate("transientScopeRoot");
    public DisposalType DisposalType => scope.DisposalType;
    public string TransientScopeDisposalReference { get; } = parentContainer.TransientScopeDisposalReference;
    public IFunctionCallNode? Initialization { get; } = initialization;
}