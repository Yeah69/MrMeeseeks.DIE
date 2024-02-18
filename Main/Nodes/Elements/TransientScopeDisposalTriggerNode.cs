using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface ITransientScopeDisposalTriggerNode : IElementNode
{
    void CheckSynchronicity();
}

internal sealed partial class TransientScopeDisposalTriggerNode : ITransientScopeDisposalTriggerNode
{
    private readonly bool _disposalHookIsSync;
    private readonly IContainerNode _parentContainer;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly WellKnownTypes _wellKnownTypes;

    public TransientScopeDisposalTriggerNode(
        INamedTypeSymbol disposableType,
        
        IContainerWideContext containerWideContext,
        IContainerNode parentContainer,
        ILocalDiagLogger localDiagLogger,
        IReferenceGenerator referenceGenerator)
    {
        _parentContainer = parentContainer;
        _localDiagLogger = localDiagLogger;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        TypeFullName = disposableType.FullName();
        Reference = referenceGenerator.Generate(disposableType);
        _disposalHookIsSync = CustomSymbolEqualityComparer.Default.Equals(
            _wellKnownTypes.IDisposable,
            disposableType);
    }

    public void Build(PassedContext passedContext) { }

    public string TypeFullName { get; }
    public string Reference { get; }
    public void CheckSynchronicity()
    {
        if (_disposalHookIsSync 
            && _wellKnownTypes.IAsyncDisposable is not null
            && _parentContainer.DisposalType != DisposalType.None
            && !_parentContainer.DisposalType.HasFlag(DisposalType.Sync))
            _localDiagLogger.Error(ErrorLogData.SyncDisposalInAsyncContainerCompilationError(
                $"When container disposal is async-only, then transient scope disposal hooks of type \"{_wellKnownTypes.IDisposable.FullName()}\" aren't allowed. Please use the \"{_wellKnownTypes.IAsyncDisposable.FullName()}\" type instead."),
                Location.None);
    }
}