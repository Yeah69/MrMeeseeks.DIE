using MrMeeseeks.DIE.Extensions;
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
    private readonly Lazy<string> _reference;

    protected InitialSubDisposalNode(Lazy<string> reference) => _reference = reference;

    public void Build(PassedContext passedContext) { }
    public abstract void Accept(INodeVisitor visitor);
    
    public abstract int SubDisposalCount { get; }

    public abstract string TypeFullName { get; }
    public string Reference => _reference.Value;
}

internal sealed partial class InitialOrdinarySubDisposalNode : InitialSubDisposalNode, IInitialOrdinarySubDisposalNode
{
    private readonly Lazy<IFunctionNode> _parentFunction;

    internal InitialOrdinarySubDisposalNode(
        Lazy<IFunctionNode> parentFunction,
        Lazy<IReferenceGenerator> referenceGenerator,
        WellKnownTypes wellKnownTypes)
        : base(referenceGenerator.Select(rg => rg.Generate("subDisposal")))
    {
        _parentFunction = parentFunction;
        TypeFullName = wellKnownTypes.ConcurrentStackOfObject.FullName();
    }

    public override int SubDisposalCount => _parentFunction.Value.GetSubDisposalCount();
    public override string TypeFullName { get; }
}

internal sealed partial class InitialTransientScopeSubDisposalNode : InitialSubDisposalNode, IInitialTransientScopeSubDisposalNode
{
    private readonly Lazy<IFunctionNode> _parentFunction;

    internal InitialTransientScopeSubDisposalNode(
        Lazy<IFunctionNode> parentFunction,
        Lazy<IReferenceGenerator> referenceGenerator,
        WellKnownTypes wellKnownTypes)
        : base(referenceGenerator.Select(rg => rg.Generate("transientScopeSubDisposal")))
    {
        _parentFunction = parentFunction;
        TypeFullName = wellKnownTypes.ListOfObject.FullName();
    }

    public override int SubDisposalCount => _parentFunction.Value.GetTransientScopeDisposalCount();
    public override string TypeFullName { get; }
}