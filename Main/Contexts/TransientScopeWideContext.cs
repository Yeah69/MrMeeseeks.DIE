using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Contexts;

internal interface ITransientScopeWideContext
{
    IRangeNode Range { get; }
    IUserDefinedElements UserDefinedElements { get; }
    ICheckTypeProperties CheckTypeProperties { get; }
}

internal sealed class TransientScopeWideContext : ITransientScopeWideContext, ITransientScopeInstance
{
    public IRangeNode Range { get; }
    public IUserDefinedElements UserDefinedElements { get; }
    public ICheckTypeProperties CheckTypeProperties { get; }

    internal TransientScopeWideContext(
        IRangeNode range,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties)
    {
        Range = range;
        UserDefinedElements = userDefinedElements;
        CheckTypeProperties = checkTypeProperties;
    }
}