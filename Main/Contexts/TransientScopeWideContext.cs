using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface ITransientScopeWideContext
{
    IUserDefinedElementsBase UserDefinedElementsBase { get; }
    ICheckTypeProperties CheckTypeProperties { get; }
}

internal class TransientScopeWideContext : ITransientScopeWideContext, ITransientScopeInstance
{
    public IUserDefinedElementsBase UserDefinedElementsBase { get; }
    public ICheckTypeProperties CheckTypeProperties { get; }

    internal TransientScopeWideContext(
        IUserDefinedElementsBase userDefinedElementsBase,
        ICheckTypeProperties checkTypeProperties)
    {
        UserDefinedElementsBase = userDefinedElementsBase;
        CheckTypeProperties = checkTypeProperties;
    }
}