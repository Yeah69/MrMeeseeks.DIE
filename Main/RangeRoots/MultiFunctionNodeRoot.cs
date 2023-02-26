using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface IMultiFunctionNodeRoot
{
    IMultiFunctionNode Function { get; }
}

internal class MultiFunctionNodeRoot : IMultiFunctionNodeRoot, IScopeRoot
{
    public MultiFunctionNodeRoot(IMultiFunctionNode function)
    {
        Function = function;
    }
    
    public IMultiFunctionNode Function { get; }
}