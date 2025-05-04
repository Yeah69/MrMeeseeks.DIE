using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal enum WrappedAsyncTransformation
{
    SameSame,
    ValueTaskFromTask,
    ValueTaskFromSync,
    ValueTaskDeeper,
    TaskFromValueTask,
    TaskFromSync,
    TaskDeeper
}

internal interface IWrappedAsyncFunctionCallNode : IFunctionCallNode
{
    WrappedAsyncTransformation Transformation { get; }
    void AdjustToCurrentCalledFunction();
}

internal sealed partial class WrappedAsyncFunctionCallNode : IWrappedAsyncFunctionCallNode
{
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly INamedTypeSymbol _someTaskType;
    private readonly ITypeSymbol _wrappedType;
    private readonly IFunctionNode _calledFunction;
    
    internal record struct Params(
        ITypeSymbol WrappedType,
        INamedTypeSymbol SomeTaskType,
        string? OwnerReference,
        IReadOnlyList<(IParameterNode, IParameterNode)> Parameters,
        IReadOnlyList<ITypeSymbol> TypeParameters,
        IElementNode CallingSubDisposal,
        IElementNode CallingTransientScopeDisposal);
    internal WrappedAsyncFunctionCallNode(
        Params parameters,
        
        IFunctionNode calledFunction,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _wellKnownTypes = wellKnownTypes;
        _wrappedType = parameters.WrappedType;
        _someTaskType = parameters.SomeTaskType;
        _calledFunction = calledFunction;
        OwnerReference = parameters.OwnerReference;
        Parameters = parameters.Parameters;
        TypeParameters = parameters.TypeParameters;
        CalledFunction = calledFunction;

        Reference = referenceGenerator.Generate(parameters.SomeTaskType);
        TypeFullName = parameters.SomeTaskType.FullName();
        SubDisposalParameter = !calledFunction.IsSubDisposalAsParameter
            ? null 
            : (parameters.CallingSubDisposal, calledFunction.SubDisposalNode);
        TransientScopeDisposalParameter = !calledFunction.IsTransientScopeDisposalAsParameter
            ? null
            : (parameters.CallingTransientScopeDisposal, calledFunction.TransientScopeDisposalNode);
    }

    public void Build(PassedContext passedContext) { }
    
    public WrappedAsyncTransformation Transformation { get; private set; }

    public void AdjustToCurrentCalledFunction()
    {
        if (_wellKnownTypes.ValueTask1 is { } valueTaskType &&
            CustomSymbolEqualityComparer.IncludeNullability.Equals(_someTaskType.OriginalDefinition, valueTaskType))
        {
            if (_calledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
            {
                Transformation = _calledFunction.ReturnedTypeFullName(ReturnTypeStatus.ValueTask) == _wrappedType.FullName() 
                    ? WrappedAsyncTransformation.ValueTaskDeeper 
                    : WrappedAsyncTransformation.SameSame;
            }
            else if (_calledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task))
            {
                Transformation = _calledFunction.ReturnedTypeFullName(ReturnTypeStatus.Task) == _wrappedType.FullName() 
                    ? WrappedAsyncTransformation.ValueTaskFromSync 
                    : WrappedAsyncTransformation.ValueTaskFromTask;
            }
            else if (_calledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Ordinary))
            {
                Transformation = WrappedAsyncTransformation.ValueTaskFromSync;
            }
            else
                throw new InvalidOperationException("Invalid return type status.");
        }
        else
        {
            if (_calledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.ValueTask))
            {
                Transformation = _calledFunction.ReturnedTypeFullName(ReturnTypeStatus.ValueTask) == _wrappedType.FullName() 
                    ? WrappedAsyncTransformation.TaskFromSync 
                    : WrappedAsyncTransformation.TaskFromValueTask;
            }
            else if (_calledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Task))
            {
                Transformation = _calledFunction.ReturnedTypeFullName(ReturnTypeStatus.Task) == _wrappedType.FullName() 
                    ? WrappedAsyncTransformation.TaskDeeper 
                    : WrappedAsyncTransformation.SameSame;
            }
            else if (_calledFunction.ReturnTypeStatus.HasFlag(ReturnTypeStatus.Ordinary))
            {
                Transformation = WrappedAsyncTransformation.TaskFromSync;
            }
            else
                throw new InvalidOperationException("Invalid return type status.");
        }
    }

    public string TypeFullName { get; }
    public string Reference { get; }
    public string? OwnerReference { get; }

    public string FunctionName(ReturnTypeStatus returnTypeStatus) => CalledFunction.Name(returnTypeStatus);
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    public (IElementNode Calling, IElementNode Called)? SubDisposalParameter { get; }
    public (IElementNode Calling, IElementNode Called)? TransientScopeDisposalParameter { get; }
    public IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    public IFunctionNode CalledFunction { get; }

    public bool Awaited => false; // never awaited, because it's a wrapped async function call
}