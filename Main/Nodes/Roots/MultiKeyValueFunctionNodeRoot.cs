using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IMultiKeyValueFunctionNodeRoot
{
    IMultiKeyValueFunctionNode Function { get; }
}

internal sealed class MultiKeyValueFunctionNodeRoot : IMultiKeyValueFunctionNodeRoot, IScopeRoot
{
    public MultiKeyValueFunctionNodeRoot(IMultiKeyValueFunctionNode function)
    {
        Function = function;
    }
    
    public IMultiKeyValueFunctionNode Function { get; }
}