using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IVoidFunctionNodeRoot
{
    IVoidFunctionNode Function { get; }
}

internal class VoidFunctionNodeRoot : IVoidFunctionNodeRoot, IScopeRoot
{
    public VoidFunctionNodeRoot(IVoidFunctionNode function)
    {
        Function = function;
    }
    
    public IVoidFunctionNode Function { get; }
}