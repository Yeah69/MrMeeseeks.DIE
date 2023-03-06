using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Contexts;

internal interface ITransientScopeWideContext
{
    IUserDefinedElementsBase UserDefinedElementsBase { get; }
}

internal class TransientScopeWideContext : ITransientScopeWideContext, ITransientScopeInstance
{
    public IUserDefinedElementsBase UserDefinedElementsBase { get; }

    internal TransientScopeWideContext(
        IUserDefinedElementsBase userDefinedElementsBase)
    {
        UserDefinedElementsBase = userDefinedElementsBase;
    }
}