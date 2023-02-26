using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IScopeNodeRoot
{
    IScopeNode Scope { get; }
}

internal class ScopeNodeRoot : IScopeNodeRoot, ITransientScopeRoot
{
    public IScopeNode Scope { get; }

    public ScopeNodeRoot(
        IScopeInfo scopeInfo,
        IScopeNode scope)
    {
        Scope = scope;
    }
}