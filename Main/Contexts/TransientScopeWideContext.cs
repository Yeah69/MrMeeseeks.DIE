using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Contexts;

internal interface ITransientScopeWideContext
{
    IRangeNode Range { get; }
    ICheckTypeProperties CheckTypeProperties { get; }
}

internal sealed class TransientScopeWideContext : ITransientScopeWideContext, ITransientScopeInstance
{
    public IRangeNode Range { get; }
    public ICheckTypeProperties CheckTypeProperties { get; }

    internal TransientScopeWideContext(
        IRangeNode range,
        ICheckTypeProperties checkTypeProperties)
    {
        Range = range;
        CheckTypeProperties = checkTypeProperties;
    }
}