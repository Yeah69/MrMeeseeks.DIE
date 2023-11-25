using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IMultiKeyValueFunctionNodeRoot
{
    IMultiKeyValueFunctionNode Function { get; }
}

internal class MultiKeyValueFunctionNodeRoot(IMultiKeyValueFunctionNode function) : IMultiKeyValueFunctionNodeRoot,
    IScopeRoot
{
    public IMultiKeyValueFunctionNode Function { get; } = function;
}