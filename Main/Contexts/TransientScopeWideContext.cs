using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Contexts;

internal interface ITransientScopeWideContext
{
    IRangeNode Range { get; }
    IUserDefinedElementsBase UserDefinedElementsBase { get; }
    ICheckTypeProperties CheckTypeProperties { get; }
}

internal class TransientScopeWideContext : ITransientScopeWideContext, ITransientScopeInstance
{
    public IRangeNode Range { get; }
    public IUserDefinedElementsBase UserDefinedElementsBase { get; }
    public ICheckTypeProperties CheckTypeProperties { get; }

    internal TransientScopeWideContext(
        IRangeNode range,
        IUserDefinedElementsBase userDefinedElementsBase,
        ICheckTypeProperties checkTypeProperties)
    {
        Range = range;
        UserDefinedElementsBase = userDefinedElementsBase;
        CheckTypeProperties = checkTypeProperties;
    }
}