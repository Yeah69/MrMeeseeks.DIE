using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface IRangedInstanceFunctionNodeRoot
{
    IRangedInstanceFunctionNode Function { get; }
}

internal class RangedInstanceFunctionNodeRoot : IRangedInstanceFunctionNodeRoot, IScopeRoot
{
    public RangedInstanceFunctionNodeRoot(IRangedInstanceFunctionNode function)
    {
        Function = function;
    }
    
    public IRangedInstanceFunctionNode Function { get; }
}