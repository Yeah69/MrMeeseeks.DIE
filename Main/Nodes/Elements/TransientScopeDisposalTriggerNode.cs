using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface ITransientScopeDisposalTriggerNode : IElementNode
{
}

internal class TransientScopeDisposalTriggerNode : ITransientScopeDisposalTriggerNode
{
    public TransientScopeDisposalTriggerNode(
        INamedTypeSymbol disposableType,
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = disposableType.FullName();
        Reference = referenceGenerator.Generate(disposableType);
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => 
        nodeVisitor.VisitTransientScopeDisposalTriggerNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
}