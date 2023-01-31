using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Elements.Tasks;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IFunctionNode : INode
{
    Accessibility? Accessibility { get; }
    SynchronicityDecision SynchronicityDecision { get; }
    string Name { get; }
    IReadOnlyList<(string, TypeKey, IParameterNode)> Parameters { get; }
    ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)> Overrides { get; }
    string ReturnedTypeFullName { get; }
    void RegisterAsyncWrapping(IPotentiallyAwaitedNode potentiallyAwaitedNode, ITaskNodeBase taskNodeBase);
    void OnAwait(IPotentiallyAwaitedNode potentiallyAwaitedNode);
    void CheckSynchronicity();
    void ForceToAsync();
    string? AsyncTypeFullName { get; }
    string RangeFullName { get; }
    string DisposedPropertyReference { get; }

    IFunctionCallNode CreateCall(string? ownerReference, IFunctionNode callingFunction);
    IScopeCallNode CreateScopeCall(string containerParameter, string transientScopeInterfaceParameter, IRangeNode callingRange, IFunctionNode callingFunction, IScopeNode scopeNode);
    ITransientScopeCallNode CreateTransientScopeCall(string containerParameter, IFunctionNode callingFunction, ITransientScopeNode scopeNode);
}