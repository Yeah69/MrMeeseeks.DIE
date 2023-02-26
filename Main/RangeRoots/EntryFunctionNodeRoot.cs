using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface IEntryFunctionNodeRoot
{
    IEntryFunctionNode Function { get; }
}

internal class EntryFunctionNodeRoot : IEntryFunctionNodeRoot, IScopeRoot
{
    public EntryFunctionNodeRoot(IEntryFunctionNode function)
    {
        Function = function;
    }
    
    public IEntryFunctionNode Function { get; }
}