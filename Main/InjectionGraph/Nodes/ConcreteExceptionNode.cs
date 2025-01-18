using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal class ConcreteExceptionNode : IConcreteNode, IContainerInstance
{
    public IReadOnlyList<TypeNode> ConnectIfNotAlready(EdgeContext context) => Array.Empty<TypeNode>();
}