

using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface IContainerInfoContext
{
    IContainerInfo ContainerInfo { get; }
}

internal class ContainerInfoContext(IContainerInfo containerInfo) : IContainerInfoContext, IContainerInstance
{
    public IContainerInfo ContainerInfo { get; } = containerInfo;
}