using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface ITransientScopeCallNode : IScopeCallNodeBase
{
    string? ContainerReference { get; }
    string TransientScopeDisposalReference { get; }
}

internal sealed partial class TransientScopeCallNode : ScopeCallNodeBase, ITransientScopeCallNode
{
    internal TransientScopeCallNode(
        string containerParameter, 
        ITransientScopeNode scope,
        IContainerNode parentContainer,
        IRangeNode callingRange,
        IFunctionNode calledFunction,
        ITypeSymbol callSideType,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReadOnlyList<ITypeSymbol> typeParameters,
        IFunctionCallNode? initialization,
        ScopeCallNodeOuterMapperParam outerMapperParam,
        
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
        ContainerReference = callingRange.ContainerReference;
        TransientScopeDisposalReference = parentContainer.TransientScopeDisposalReference;
        AdditionalPropertiesForConstruction = [(scope.ContainerReference ?? "", containerParameter)];
    }

    protected override (string Name, string Reference)[] AdditionalPropertiesForConstruction { get; }

    public string? ContainerReference { get; }
    public string TransientScopeDisposalReference { get; }
}