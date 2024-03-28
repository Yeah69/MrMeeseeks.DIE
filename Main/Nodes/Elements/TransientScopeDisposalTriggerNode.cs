using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface ITransientScopeDisposalTriggerNode : IElementNode
{
}

internal sealed partial class TransientScopeDisposalTriggerNode : ITransientScopeDisposalTriggerNode
{
    internal TransientScopeDisposalTriggerNode(
        INamedTypeSymbol disposableType,
        
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = disposableType.FullName();
        Reference = referenceGenerator.Generate(disposableType);
    }

    public void Build(PassedContext passedContext) { }

    public string TypeFullName { get; }
    public string Reference { get; }
}