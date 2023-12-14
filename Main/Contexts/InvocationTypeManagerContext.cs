using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Interceptors;

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