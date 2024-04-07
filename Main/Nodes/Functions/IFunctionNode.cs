using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IFunctionNode : INode
{
    Accessibility? Accessibility { get; }
    SynchronicityDecision SynchronicityDecision { get; }
    string Name { get; }
    IReadOnlyList<(ITypeSymbol Type, IParameterNode Node)> Parameters { get; }
    ImmutableDictionary<ITypeSymbol, IParameterNode> Overrides { get; }
    IReadOnlyList<ITypeParameterSymbol> TypeParameters { get; }
    string ReturnedTypeFullName { get; }
    string ReturnedTypeNameNotWrapped { get; }
    string Description { get; }
    HashSet<IFunctionNode> CalledFunctions { get; }
    IEnumerable<IFunctionNode> CalledFunctionsOfSameRange { get; }
    IEnumerable<IInitializedInstanceNode> UsedInitializedInstance { get; }
    void RegisterAwaitableNode(IAwaitableNode awaitableNode);
    void RegisterCalledFunction(IFunctionNode calledFunction);
    void RegisterCallingFunction(IFunctionNode callingFunction);
    void RegisterUsedInitializedInstance(IInitializedInstanceNode initializedInstance);
    void CheckSynchronicity();
    void ForceToAsync();
    string RangeFullName { get; }
    string DisposedPropertyReference { get; }
    IReadOnlyList<ILocalFunctionNode> LocalFunctions { get; }
    void AddLocalFunction(ILocalFunctionNode function);
    string? ExplicitInterfaceFullName { get; }
    string ResolutionCounterReference { get; }
    IElementNode SubDisposalNode { get; }
    IElementNode TransientScopeDisposalNode { get; }
    /// <summary>
    /// Sub disposal is passed is either passed as parameter or initialized in the function body (in entry functions).
    /// </summary>
    bool IsSubDisposalAsParameter { get; }
    /// <summary>
    /// Transient scope disposal is passed is either passed as parameter or initialized in the function body (in entry functions).
    /// </summary>
    bool IsTransientScopeDisposalAsParameter { get; }
    string DisposalCollectionReference { get; }
    string? ContainerReference { get; }
    string TransientScopeDisposalReference { get; }
    string DisposeExceptionHandlingMethodName { get; }
    string DisposeExceptionHandlingAsyncMethodName { get; }

    IFunctionCallNode CreateCall(
        ITypeSymbol callSideType,
        string? ownerReference,
        IFunctionNode callingFunction,
        IReadOnlyList<ITypeSymbol> typeParameters);
    IWrappedAsyncFunctionCallNode CreateAsyncCall(
        ITypeSymbol wrappedType,
        string? ownerReference,
        SynchronicityDecision synchronicity,
        IFunctionNode callingFunction,
        IReadOnlyList<ITypeSymbol> typeParameters);
    IScopeCallNode CreateScopeCall(
        ITypeSymbol callSideType, 
        string containerParameter, 
        string transientScopeInterfaceParameter, 
        IRangeNode callingRange, 
        IFunctionNode callingFunction, 
        IScopeNode scopeNode, 
        IReadOnlyList<ITypeSymbol> typeParameters,
        IElementNodeMapperBase scopeImplementationMapper);
    ITransientScopeCallNode CreateTransientScopeCall(
        ITypeSymbol callSideType,
        string containerParameter,
        IRangeNode callingRange,
        IFunctionNode callingFunction,
        ITransientScopeNode scopeNode,
        IReadOnlyList<ITypeSymbol> typeParameters,
        IElementNodeMapperBase transientScopeImplementationMapper);
    bool CheckIfReturnedType(ITypeSymbol type);

    bool TryGetReusedNode(ITypeSymbol type, out IReusedNode? reusedNode);
    void AddReusedNode(ITypeSymbol type, IReusedNode reusedNode);
    INodeGenerator GetGenerator();
}