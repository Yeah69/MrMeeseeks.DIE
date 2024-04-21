using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IInitialSubDisposalNode : IElementNode
{
    int SubDisposalCount { get; }
}

internal interface IInitialOrdinarySubDisposalNode : IInitialSubDisposalNode;

internal interface IInitialTransientScopeSubDisposalNode : IInitialSubDisposalNode;

internal abstract class InitialSubDisposalNode : IInitialSubDisposalNode
{

    protected InitialSubDisposalNode(
        string reference,
        WellKnownTypes wellKnownTypes)
    {
        TypeFullName = wellKnownTypes.ListOfObject.FullName();
        Reference = reference;
    }
    
    public void Build(PassedContext passedContext) { }
    public abstract void Accept(INodeVisitor visitor);
    
    public abstract int SubDisposalCount { get; }

    public string TypeFullName { get; }
    public string Reference { get; }
}

internal sealed partial class InitialOrdinarySubDisposalNode : InitialSubDisposalNode, IInitialOrdinarySubDisposalNode
{
    private readonly Lazy<IFunctionNode> _parentFunction;

    internal InitialOrdinarySubDisposalNode(
        Lazy<IFunctionNode> parentFunction,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
        : base(referenceGenerator.Generate("subDisposal"), wellKnownTypes) =>
        _parentFunction = parentFunction;

    public override int SubDisposalCount => _parentFunction.Value.GetSubDisposalCount();
}

internal sealed partial class InitialTransientScopeSubDisposalNode : InitialSubDisposalNode, IInitialTransientScopeSubDisposalNode
{
    private readonly Lazy<IFunctionNode> _parentFunction;

    internal InitialTransientScopeSubDisposalNode(
        Lazy<IFunctionNode> parentFunction,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
        : base(referenceGenerator.Generate("transientScopeSubDisposal"), wellKnownTypes) =>
        _parentFunction = parentFunction;

    public override int SubDisposalCount => _parentFunction.Value.GetTransientScopeDisposalCount();
}