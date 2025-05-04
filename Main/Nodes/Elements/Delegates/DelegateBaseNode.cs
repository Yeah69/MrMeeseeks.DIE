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
    private readonly IReadOnlyList<ITypeSymbol> _typeParameters;
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
        _typeParameters = typeParameters;
        _localDiagLogger = localDiagLogger;
        _parentContainer = parentContainer;
        Reference = referenceGenerator.Generate(delegateTypes.Inner);
        TypeFullName = delegateTypes.Outer.FullName();
        _innerType = delegateTypes.Inner.TypeArguments.Last();
    }

    public virtual void Build(PassedContext passedContext) => 
        _parentContainer.RegisterDelegateBaseNode(this);

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }

    public string MethodGroup 
    {
        get
        {
            var genericsSuffix = _typeParameters.Any() ? $"<{string.Join(", ", _typeParameters.Select(p => p.FullName()))}>" : string.Empty;
            var functionName = _function.ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask)
                ? _function.Name(ReturnTypeStatus.ValueTask)
                : _function.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task)
                    ? _function.Name(ReturnTypeStatus.Task)
                    : _function.Name(ReturnTypeStatus.Ordinary);
            return $"{functionName}{genericsSuffix}";
        }
    }
    public void CheckSynchronicity()
    {
        if (_function.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Ordinary)     && Equals(_function.ReturnedTypeFullName(ReturnTypeStatus.Ordinary), _innerType.FullName())
            || _function.ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask) && Equals(_function.ReturnedTypeFullName(ReturnTypeStatus.ValueTask), _innerType.FullName())
            || _function.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task)      && Equals(_function.ReturnedTypeFullName(ReturnTypeStatus.Task), _innerType.FullName()))
            return;
        _localDiagLogger.Error(ErrorLogData.SyncToAsyncCallCompilationError(
            "Func/Lazy/ThreadLocal injections need to have a sync function generated or its return type should be wrapped by a ValueTask/Task."),
            Location.None);
    }
}