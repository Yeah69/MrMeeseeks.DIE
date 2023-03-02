

using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface IContainerInfoContext
{
    IContainerInfo ContainerInfo { get; }
}

internal class ContainerInfoContext : IContainerInfoContext, IContainerInstance
{
    public ContainerInfoContext(
        IContainerInfo containerInfo)
    {
        ContainerInfo = containerInfo;
    }

    public IContainerInfo ContainerInfo { get; }
}