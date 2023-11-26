using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Contexts;

internal interface IInvocationTypeManagerContext
{
    IInvocationTypeManager InvocationTypeManager { get; }
}

internal class InvocationTypeManagerContext(
        IInvocationTypeManager invocationTypeManager)
    : IInvocationTypeManagerContext
{
    public IInvocationTypeManager InvocationTypeManager { get; } = invocationTypeManager;
}