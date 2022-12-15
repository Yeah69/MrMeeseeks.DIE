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
}

internal class ScopeCallNode : FunctionCallNode, IScopeCallNode
{
    public override void Accept(INodeVisitor nodeVisitor)
    {
        nodeVisitor.VisitScopeCallNode(this);
    }

    public ScopeCallNode(
        string containerParameter, 
        string transientScopeInterfaceParameter,
        IScopeNode scope,
        IFunctionNode calledFunction, 
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters, 
        IReferenceGenerator referenceGenerator) 
        : base(null, calledFunction, parameters, referenceGenerator)
    {
        ContainerParameter = containerParameter;
        TransientScopeInterfaceParameter = transientScopeInterfaceParameter;
        ScopeFullName = scope.FullName;
        ScopeReference = referenceGenerator.Generate("scopeRoot");
    }

    public override string OwnerReference => ScopeReference;

    public string ContainerParameter { get; }
    public string TransientScopeInterfaceParameter { get; }
    public string ScopeFullName { get; }
    public string ScopeReference { get; }
}