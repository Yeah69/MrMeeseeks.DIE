using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IRangedInstanceFunctionNodeRoot
{
    IRangedInstanceFunctionNode Function { get; }
}

internal sealed class RangedInstanceFunctionNodeRoot : IRangedInstanceFunctionNodeRoot, IScopeRoot
{
    public RangedInstanceFunctionNodeRoot(IRangedInstanceFunctionNode function)
    {
        Function = function;
    }
    
    public IRangedInstanceFunctionNode Function { get; }
}