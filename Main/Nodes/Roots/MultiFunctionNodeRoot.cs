using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IMultiFunctionNodeRoot
{
    IMultiFunctionNode Function { get; }
}

internal class MultiFunctionNodeRoot(IMultiFunctionNode function) : IMultiFunctionNodeRoot, IScopeRoot
{
    public IMultiFunctionNode Function { get; } = function;
}