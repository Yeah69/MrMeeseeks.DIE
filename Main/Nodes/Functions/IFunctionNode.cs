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
    string? AsyncTypeFullName { get; }
    string RangeFullName { get; }
    string DisposedPropertyReference { get; }
    IReadOnlyList<ILocalFunctionNode> LocalFunctions { get; }
    void AddLocalFunction(ILocalFunctionNode function);
    string? ExplicitInterfaceFullName { get; }

    IFunctionCallNode CreateCall(string? ownerReference, IFunctionNode callingFunction);
    IAsyncFunctionCallNode CreateAsyncCall(ITypeSymbol wrappedType, string? ownerReference, SynchronicityDecision synchronicity, IFunctionNode callingFunction);
    IScopeCallNode CreateScopeCall(string containerParameter, string transientScopeInterfaceParameter, IRangeNode callingRange, IFunctionNode callingFunction, IScopeNode scopeNode);
    ITransientScopeCallNode CreateTransientScopeCall(string containerParameter, IRangeNode callingRange, IFunctionNode callingFunction, ITransientScopeNode scopeNode);
    bool CheckIfReturnedType(ITypeSymbol type);

    bool TryGetReusedNode(ITypeSymbol type, out IReusedNode? reusedNode);
    void AddReusedNode(ITypeSymbol type, IReusedNode reusedNode);
}