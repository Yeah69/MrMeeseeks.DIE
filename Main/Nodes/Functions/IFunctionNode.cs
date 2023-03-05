using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IOnAwait
{
    void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode);
}

internal interface IFunctionNode : INode, IOnAwait
{
    Accessibility? Accessibility { get; }
    SynchronicityDecision SynchronicityDecision { get; }
    string Name { get; }
    IReadOnlyList<(ITypeSymbol Type, IParameterNode Node)> Parameters { get; }
    ImmutableDictionary<ITypeSymbol, IParameterNode> Overrides { get; }
    string ReturnedTypeFullName { get; }
    void RegisterAsyncWrapping(IPotentiallyAwaitedNode potentiallyAwaitedNode, ITaskNodeBase taskNodeBase);
    string Description { get; }
    HashSet<IFunctionNode> CalledFunctions { get; }
    void RegisterCalledFunction(IFunctionNode calledFunction);
    void CheckSynchronicity();
    void ForceToAsync();
    string? AsyncTypeFullName { get; }
    string RangeFullName { get; }
    string DisposedPropertyReference { get; }
    IReadOnlyList<ILocalFunctionNode> LocalFunctions { get; }
    void AddLocalFunction(ILocalFunctionNode function);
    string? ExplicitInterfaceFullName { get; }

    IFunctionCallNode CreateCall(string? ownerReference, IFunctionNode callingFunction, IOnAwait onAwait);
    IScopeCallNode CreateScopeCall(string containerParameter, string transientScopeInterfaceParameter, IRangeNode callingRange, IFunctionNode callingFunction, IScopeNode scopeNode);
    ITransientScopeCallNode CreateTransientScopeCall(string containerParameter, IRangeNode callingRange, IFunctionNode callingFunction, ITransientScopeNode scopeNode);
    bool CheckIfReturnedType(ITypeSymbol type);
}