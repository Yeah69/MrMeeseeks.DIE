using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface ITransientScopeDisposalTriggerNode : IElementNode
{
    void CheckSynchronicity();
}

internal class TransientScopeDisposalTriggerNode : ITransientScopeDisposalTriggerNode
{
    private readonly bool _disposalHookIsSync;
    private readonly IContainerNode _parentContainer;
    private readonly WellKnownTypes _wellKnownTypes;

    public TransientScopeDisposalTriggerNode(
        INamedTypeSymbol disposableType,
        
        IContainerWideContext containerWideContext,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator)
    {
        _parentContainer = parentContainer;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        TypeFullName = disposableType.FullName();
        Reference = referenceGenerator.Generate(disposableType);
        _disposalHookIsSync = CustomSymbolEqualityComparer.Default.Equals(
            _wellKnownTypes.IDisposable,
            disposableType);
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => 
        nodeVisitor.VisitTransientScopeDisposalTriggerNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public void CheckSynchronicity()
    {
        if (_disposalHookIsSync 
            && !_parentContainer.DisposalType.HasFlag(DisposalType.Sync)
            && _parentContainer.DisposalType != DisposalType.None)
            throw new CompilationDieException(Diagnostics.SyncDisposalInAsyncContainerCompilationError(
                $"When container disposal is async-only, then transient scope disposal hooks of type \"{_wellKnownTypes.IDisposable.FullName()}\" aren't allowed. Please use the \"{_wellKnownTypes.IAsyncDisposable.FullName()}\" type instead.",
                ExecutionPhase.CodeGeneration));
    }
}