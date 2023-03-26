using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Logging;

internal interface IContainerLevelLogMessageEnhancer
{
    string Enhance(string message);
}

internal class ContainerLevelLogMessageEnhancer : IContainerLevelLogMessageEnhancer, IContainerInstance
{
    private readonly ICurrentExecutionPhase _currentExecutionPhase;
    private readonly Lazy<string> _containerPart;

    internal ContainerLevelLogMessageEnhancer(
        Lazy<IContainerNode> parentContainer,
        ICurrentExecutionPhase currentExecutionPhase)
    {
        _currentExecutionPhase = currentExecutionPhase;
        _containerPart = parentContainer.Select(c => $"[C:{c.Name}]");
    }

    public string Enhance(string message) => 
        $"[{Constants.DieAbbreviation}][P:{_currentExecutionPhase.Value.ToString()}]{_containerPart.Value}{message}";
}