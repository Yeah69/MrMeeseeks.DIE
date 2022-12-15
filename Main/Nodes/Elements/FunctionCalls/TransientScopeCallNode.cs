using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface ITransientScopeCallNode : IFunctionCallNode
{
    string ContainerParameter { get; }
    string TransientScopeFullName { get; }
    string TransientScopeReference { get; }
}

internal class TransientScopeCallNode : FunctionCallNode, ITransientScopeCallNode
{
    public override void Accept(INodeVisitor nodeVisitor)
    {
        nodeVisitor.VisitTransientScopeCallNode(this);
    }

    public TransientScopeCallNode(
        string containerParameter, 
        ITransientScopeNode scope,
        IFunctionNode calledFunction, 
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters, 
        IReferenceGenerator referenceGenerator) 
        : base(null, calledFunction, parameters, referenceGenerator)
    {
        ContainerParameter = containerParameter;
        TransientScopeFullName = scope.FullName;
        TransientScopeReference = referenceGenerator.Generate("transientScopeRoot");
    }

    public override string OwnerReference => TransientScopeReference;

    public string ContainerParameter { get; }
    public string TransientScopeFullName { get; }
    public string TransientScopeReference { get; }
}