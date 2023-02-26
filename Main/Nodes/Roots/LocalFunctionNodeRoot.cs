using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ILocalFunctionNodeRoot
{
    ILocalFunctionNode Function { get; }
}

internal class LocalFunctionNodeRoot : ILocalFunctionNodeRoot, IScopeRoot
{
    public LocalFunctionNodeRoot(ILocalFunctionNode function)
    {
        Function = function;
    }
    
    public ILocalFunctionNode Function { get; }
}