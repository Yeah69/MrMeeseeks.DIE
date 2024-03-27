using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface ITransientScopeCallNode : IScopeCallNodeBase
{
    string? ContainerReference { get; }
    string TransientScopeDisposalReference { get; }
    string TransientScopeDisposalReference_Old { get; }
}

internal sealed partial class TransientScopeCallNode : ScopeCallNodeBase, ITransientScopeCallNode
{
    internal record struct Params(
        ITypeSymbol CallSideType, 
        string ContainerParameter,
        ITransientScopeNode Scope,
        IRangeNode CallingRange,
        IElementNode CallingTransientScopeDisposal,
        IReadOnlyList<(IParameterNode, IParameterNode)> Parameters,
        IReadOnlyList<ITypeSymbol> TypeParameters,
        IFunctionCallNode? Initialization,
        ScopeCallNodeOuterMapperParam OuterMapperParam);
    internal TransientScopeCallNode(
        Params parameters,
        
        IContainerNode parentContainer,
        IFunctionNode calledFunction,
        IReferenceGenerator referenceGenerator) 
        : base(
            parameters.CallSideType,
            parameters.Scope,
            parameters.Parameters,
            parameters.TypeParameters,
            parameters.Initialization,
            parameters.OuterMapperParam,
            parameters.CallingTransientScopeDisposal,
            calledFunction,
            referenceGenerator)
    {
        ContainerReference = parameters.CallingRange.ContainerReference;
        TransientScopeDisposalReference = parentContainer.TransientScopeDisposalReference;
        TransientScopeDisposalReference_Old = parentContainer.TransientScopeDisposalReference_Old;
        AdditionalPropertiesForConstruction = [(parameters.Scope.ContainerReference ?? "", parameters.ContainerParameter)];
    }

    protected override (string Name, string Reference)[] AdditionalPropertiesForConstruction { get; }

    public string? ContainerReference { get; }
    public string TransientScopeDisposalReference { get; }
    public string TransientScopeDisposalReference_Old { get; }
}