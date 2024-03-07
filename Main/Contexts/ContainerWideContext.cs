

using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface IContainerWideContext
{
    Compilation Compilation { get; }
}

internal sealed class ContainerWideContext : IContainerWideContext, IContainerInstance
{
    internal ContainerWideContext(
        Compilation compilation)
    {
        Compilation = compilation;
    }

    public Compilation Compilation { get; }
}