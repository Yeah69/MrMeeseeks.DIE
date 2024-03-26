using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IInitialSubDisposalNode : IElementNode;

internal sealed partial class InitialSubDisposalNode : IInitialSubDisposalNode
{
    internal InitialSubDisposalNode(
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        TypeFullName = wellKnownTypes.ListOfObject.FullName();
        Reference = referenceGenerator.Generate("subDisposal");
    }
    
    public void Build(PassedContext passedContext) { }

    public string TypeFullName { get; }
    public string Reference { get; }
}