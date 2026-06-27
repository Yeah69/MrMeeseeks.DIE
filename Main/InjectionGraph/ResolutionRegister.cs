using System.Threading;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ResolutionRegister : IContainerInstance
{
    private int _resolutionId;
    
    internal int GetNewResolutionId() =>
        Interlocked.Increment(ref _resolutionId);
}