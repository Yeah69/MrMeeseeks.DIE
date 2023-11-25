using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IRangedInstanceInterfaceFunctionNodeRoot
{
    IRangedInstanceInterfaceFunctionNode Function { get; }
}

internal class RangedInstanceInterfaceFunctionNodeRoot
    (IRangedInstanceInterfaceFunctionNode function) : IRangedInstanceInterfaceFunctionNodeRoot, IScopeRoot
{
    public IRangedInstanceInterfaceFunctionNode Function { get; } = function;
}