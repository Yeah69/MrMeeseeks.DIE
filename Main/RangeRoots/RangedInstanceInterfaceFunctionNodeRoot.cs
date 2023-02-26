using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface IRangedInstanceInterfaceFunctionNodeRoot
{
    IRangedInstanceInterfaceFunctionNode Function { get; }
}

internal class RangedInstanceInterfaceFunctionNodeRoot : IRangedInstanceInterfaceFunctionNodeRoot, IScopeRoot
{
    public RangedInstanceInterfaceFunctionNodeRoot(IRangedInstanceInterfaceFunctionNode function)
    {
        Function = function;
    }
    
    public IRangedInstanceInterfaceFunctionNode Function { get; }
}