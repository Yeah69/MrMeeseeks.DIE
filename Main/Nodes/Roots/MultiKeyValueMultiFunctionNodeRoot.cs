using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IMultiKeyValueMultiFunctionNodeRoot
{
    IMultiKeyValueMultiFunctionNode Function { get; }
}

internal class MultiKeyValueMultiFunctionNodeRoot
    (IMultiKeyValueMultiFunctionNode function) : IMultiKeyValueMultiFunctionNodeRoot, IScopeRoot
{
    public IMultiKeyValueMultiFunctionNode Function { get; } = function;
}