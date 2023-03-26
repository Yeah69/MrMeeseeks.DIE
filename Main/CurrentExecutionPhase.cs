using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE;

internal interface ICurrentExecutionPhase
{
    ExecutionPhase Value { get; }
}

internal interface ICurrentExecutionPhaseSetter
{
    ExecutionPhase Value { set; }
}

internal class CurrentExecutionPhase : ICurrentExecutionPhase, ICurrentExecutionPhaseSetter, IContainerInstance
{
    public ExecutionPhase Value { get; set; } = ExecutionPhase.ContainerValidation;
}