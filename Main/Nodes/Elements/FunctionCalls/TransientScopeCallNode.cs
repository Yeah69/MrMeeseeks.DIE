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

internal partial class TransientScopeCallNode : FunctionCallNode, ITransientScopeCallNode
{
    private readonly ITransientScopeNode _scope;

    public TransientScopeCallNode(
        string containerParameter, 
        ITransientScopeNode scope,
        IContainerNode parentContainer,
        IRangeNode callingRange,
        IFunctionNode calledFunction,
        ITypeSymbol callSideType,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReadOnlyList<ITypeSymbol> typeParameters,
        IFunctionCallNode? initialization,
        
        IReferenceGenerator referenceGenerator) 
        : base(null, calledFunction, callSideType, parameters, typeParameters, referenceGenerator)
    {
        _scope = scope;
        ContainerParameter = containerParameter;
        Initialization = initialization;
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
    public IFunctionCallNode? Initialization { get; }
}