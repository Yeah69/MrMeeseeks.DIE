using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IRangedInstanceInterfaceFunctionNodeRoot
{
    IRangedInstanceInterfaceFunctionNode Function { get; }
}

internal sealed class RangedInstanceInterfaceFunctionNodeRoot : IRangedInstanceInterfaceFunctionNodeRoot, IScopeRoot
{
    public RangedInstanceInterfaceFunctionNodeRoot(IRangedInstanceInterfaceFunctionNode function)
    {
        Function = function;
    }
    
    public IRangedInstanceInterfaceFunctionNode Function { get; }
}