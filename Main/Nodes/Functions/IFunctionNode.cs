using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IFunctionNode : INode
{
    Accessibility? Accessibility { get; }
    ReturnTypeStatus ReturnTypeStatus { get; }
    AsyncAwaitStatus AsyncAwaitStatus { get; }
    string Name(ReturnTypeStatus returnTypeStatus);
    (IReadOnlyList<IFunctionNode> Calling, IReadOnlyList<IFunctionNode> Called) MakeTaskBasedOnly();
    IReadOnlyList<IFunctionNode> MakeTaskBasedToo();
    AsyncSingleReturnStrategy SelectAsyncSingleReturnStrategy(ReturnTypeStatus returnTypeStatus, bool isAsyncAwait);
    IReadOnlyList<(ITypeSymbol Type, IParameterNode Node)> Parameters { get; }
    ImmutableDictionary<ITypeSymbol, IParameterNode> Overrides { get; }
    IReadOnlyList<ITypeParameterSymbol> TypeParameters { get; }
    string ReturnedTypeFullName(ReturnTypeStatus returnTypeStatus);
    string ReturnedTypeNameNotWrapped { get; }
    string Description { get; }
    HashSet<IFunctionNode> CalledFunctions { get; }
    IEnumerable<IFunctionNode> CalledFunctionsOfSameRange { get; }
    IEnumerable<IInitializedInstanceNode> UsedInitializedInstance { get; }
    void RegisterCalledFunction(IFunctionNode calledFunction, bool isNotAsyncWrapped);
    void RegisterCallingFunction(IFunctionNode callingFunction);
    void RegisterUsedInitializedInstance(IInitializedInstanceNode initializedInstance);
    void AddOneToSubDisposalCount();
    int GetSubDisposalCount();
    void AddOneToTransientScopeDisposalCount();
    int GetTransientScopeDisposalCount();
    string RangeFullName { get; }
    IReadOnlyList<ILocalFunctionNode> LocalFunctions { get; }
    void AddLocalFunction(ILocalFunctionNode function);
    string? ExplicitInterfaceFullName { get; }
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

    IFunctionCallNode CreateCall(
        ITypeSymbol callSideType,
        string? ownerReference,
        IFunctionNode callingFunction,
        IReadOnlyList<ITypeSymbol> typeParameters);
    IWrappedAsyncFunctionCallNode CreateAsyncCall(
        ITypeSymbol wrappedType,
        INamedTypeSymbol someTaskType,
        string? ownerReference,
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

    bool TryGetReusedNode(ITypeSymbol type, out IReusedNode? reusedNode);
    void AddReusedNode(ITypeSymbol type, IReusedNode reusedNode);
    INodeGenerator GetGenerator();
}