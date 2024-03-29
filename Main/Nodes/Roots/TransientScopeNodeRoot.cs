using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ITransientScopeNodeRoot
{
    ITransientScopeNode TransientScope { get; }
}

internal sealed class TransientScopeNodeRoot : ITransientScopeNodeRoot, ITransientScopeRoot
{
    public ITransientScopeNode TransientScope { get; }

    public TransientScopeNodeRoot(
        ITransientScopeNode transientScope)
    {
        TransientScope = transientScope;
    }
}