using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IRangedInstanceFunctionNodeRoot
{
    IRangedInstanceFunctionNode Function { get; }
}

internal class RangedInstanceFunctionNodeRoot(IRangedInstanceFunctionNode function) : IRangedInstanceFunctionNodeRoot,
    IScopeRoot
{
    public IRangedInstanceFunctionNode Function { get; } = function;
}