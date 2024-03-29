using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IScopeNodeRoot
{
    IScopeNode Scope { get; }
}

internal sealed class ScopeNodeRoot : IScopeNodeRoot, ITransientScopeRoot
{
    public IScopeNode Scope { get; }

    public ScopeNodeRoot(
        IScopeNode scope)
    {
        Scope = scope;
    }
}