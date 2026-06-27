using MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IGraphRoot
{
    IInjectionGraphBuilder GraphBuilder { get; }
    ConcreteImplementationNodeManager ConcreteImplementationNodeManager { get; }
    ConcreteInterfaceNodeManager ConcreteInterfaceNodeManager { get; }
    ConcreteFunctorNodeManager ConcreteFunctorNodeManager { get; }
    ConcreteEnumerableNodeManager ConcreteEnumerableNodeManager { get; }
    ConcreteKeyValuePairNodeManager ConcreteKeyValuePairNodeManager { get; }
    ConcreteOverrideNodeManager ConcreteOverrideNodeManager { get; }
    ConcreteEntryFunctionNodeManager ConcreteEntryFunctionNodeManager { get; }
    ConcreteTaskNodeManager ConcreteTaskNodeManager { get; }
    ConcreteExceptionNode ConcreteExceptionNode { get; }
    TypeNodeManager TypeNodeManager { get; }
    EdgeRegistry EdgeRegistry { get; }
    Synchronicity Synchronicity { get; }
    InjectionNodeGenerator InjectionNodeGenerator { get; }
}

internal abstract class GraphRootBase : IGraphRoot
{
    internal GraphRootBase(Synchronicity synchronicity, bool isSync)
    {
        synchronicity.IsSync = isSync;
        Synchronicity = synchronicity;
    }

    public required IInjectionGraphBuilder GraphBuilder { get; init; }
    public required ConcreteImplementationNodeManager ConcreteImplementationNodeManager { get; init; }
    public required ConcreteInterfaceNodeManager ConcreteInterfaceNodeManager { get; init; }
    public required ConcreteFunctorNodeManager ConcreteFunctorNodeManager { get; init; }
    public required ConcreteEnumerableNodeManager ConcreteEnumerableNodeManager { get; init; }
    public required ConcreteKeyValuePairNodeManager ConcreteKeyValuePairNodeManager { get; init; }
    public required ConcreteOverrideNodeManager ConcreteOverrideNodeManager { get; init; }
    public required ConcreteEntryFunctionNodeManager ConcreteEntryFunctionNodeManager { get; init; }
    public required ConcreteTaskNodeManager ConcreteTaskNodeManager { get; init; }
    public required ConcreteExceptionNode ConcreteExceptionNode { get; init; }
    public required TypeNodeManager TypeNodeManager { get; init; }
    public required AsyncAdjustments AsyncAdjustments { get; init; }
    public required EdgeRegistry EdgeRegistry { get; init; }
    public Synchronicity Synchronicity { get; }
    public required InjectionNodeGenerator InjectionNodeGenerator { get; init; }
}

internal sealed class SyncGraphRoot(Synchronicity synchronicity) : GraphRootBase(synchronicity, true), IScopeRoot;

internal sealed class AsyncGraphRoot(Synchronicity synchronicity) : GraphRootBase(synchronicity, false), IScopeRoot;

internal sealed class SyncGraphHolder : IContainerInstance
{
    internal required SyncGraphRoot Value { get; init; }
}

internal sealed class AsyncGraphHolder : IContainerInstance
{
    internal required AsyncGraphRoot Value { get; init; }
}

internal sealed class Synchronicity : IScopeInstance
{
    internal bool IsSync { get; set; }
}