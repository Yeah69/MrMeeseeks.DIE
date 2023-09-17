using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IMultiKeyValueMultiFunctionNodeRoot
{
    IMultiKeyValueMultiFunctionNode Function { get; }
}

internal class MultiKeyValueMultiFunctionNodeRoot : IMultiKeyValueMultiFunctionNodeRoot, IScopeRoot
{
    public MultiKeyValueMultiFunctionNodeRoot(IMultiKeyValueMultiFunctionNode function)
    {
        Function = function;
    }
    
    public IMultiKeyValueMultiFunctionNode Function { get; }
}