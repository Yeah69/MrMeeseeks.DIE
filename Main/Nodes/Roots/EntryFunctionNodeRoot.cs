using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IEntryFunctionNodeRoot
{
    IEntryFunctionNode Function { get; }
}

internal sealed class EntryFunctionNodeRoot : IEntryFunctionNodeRoot, IScopeRoot
{
    public EntryFunctionNodeRoot(IEntryFunctionNode function)
    {
        Function = function;
    }
    
    public IEntryFunctionNode Function { get; }
}