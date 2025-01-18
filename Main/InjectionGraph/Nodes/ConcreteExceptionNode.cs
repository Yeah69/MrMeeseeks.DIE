using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal class ConcreteExceptionNode : IConcreteNode, IContainerInstance
{
    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context) => 
        Array.Empty<(TypeNode TypeNode, Location Location)>();
}