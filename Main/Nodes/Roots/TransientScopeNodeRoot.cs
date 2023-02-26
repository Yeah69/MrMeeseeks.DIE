using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ITransientScopeNodeRoot
{
    ITransientScopeNode TransientScope { get; }
}

internal class TransientScopeNodeRoot : ITransientScopeNodeRoot, ITransientScopeRoot
{
    public ITransientScopeNode TransientScope { get; }

    public TransientScopeNodeRoot(
        IScopeInfo scopeInfo,
        ITransientScopeNode transientScope)
    {
        TransientScope = transientScope;
    }
}