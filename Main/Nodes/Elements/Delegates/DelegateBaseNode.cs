using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IDelegateBaseNode : IElementNode
{
    string MethodGroup { get; }
    void CheckSynchronicity();
}

internal abstract class DelegateBaseNode : IDelegateBaseNode
{
    private readonly ILocalFunctionNode _function;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly IContainerNode _parentContainer;
    private readonly ITypeSymbol _innerType;

    internal DelegateBaseNode(
        (INamedTypeSymbol Outer, INamedTypeSymbol Inner) delegateTypes,
        ILocalFunctionNode function,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        ILocalDiagLogger localDiagLogger,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator)
    {
        _function = function;
        _localDiagLogger = localDiagLogger;
        _parentContainer = parentContainer;
        var genericsSuffix = typeParameters.Any() ? $"<{string.Join(", ", typeParameters.Select(p => p.FullName()))}>" : string.Empty;
        MethodGroup = $"{function.Name}{genericsSuffix}";
        Reference = referenceGenerator.Generate(delegateTypes.Inner);
        TypeFullName = delegateTypes.Outer.FullName();
        _innerType = delegateTypes.Inner.TypeArguments.Last();
    }

    public virtual void Build(PassedContext passedContext) => 
        _parentContainer.RegisterDelegateBaseNode(this);

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    
    public string MethodGroup { get; }
    public void CheckSynchronicity()
    {
        if (!Equals(_function.ReturnedTypeFullName, _innerType.FullName()))
            _localDiagLogger.Error(ErrorLogData.SyncToAsyncCallCompilationError(
                "Func/Lazy injections need to have a sync function generated or its return type should be wrapped by a ValueTask/Task."),
                Location.None);
    }
}