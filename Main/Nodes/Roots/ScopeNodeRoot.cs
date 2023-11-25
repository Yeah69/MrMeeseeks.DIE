using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IScopeNodeRoot
{
    IScopeNode Scope { get; }
}

internal class ScopeNodeRoot(IScopeNode scope) : IScopeNodeRoot, ITransientScopeRoot
{
    public IScopeNode Scope { get; } = scope;
}