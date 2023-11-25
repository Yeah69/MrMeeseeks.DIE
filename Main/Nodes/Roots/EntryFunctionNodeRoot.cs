using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IEntryFunctionNodeRoot
{
    IEntryFunctionNode Function { get; }
}

internal class EntryFunctionNodeRoot(IEntryFunctionNode function) : IEntryFunctionNodeRoot, IScopeRoot
{
    public IEntryFunctionNode Function { get; } = function;
}