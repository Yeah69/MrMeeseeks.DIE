using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IVoidFunctionNodeRoot
{
    IVoidFunctionNode Function { get; }
}

internal class VoidFunctionNodeRoot(IVoidFunctionNode function) : IVoidFunctionNodeRoot, IScopeRoot
{
    public IVoidFunctionNode Function { get; } = function;
}