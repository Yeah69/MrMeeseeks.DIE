using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface ICreateTransientScopeFunctionNodeRoot
{
    ICreateTransientScopeFunctionNode Function { get; }
}

internal class CreateTransientScopeFunctionNodeRoot : ICreateTransientScopeFunctionNodeRoot, IScopeRoot
{
    public CreateTransientScopeFunctionNodeRoot(ICreateTransientScopeFunctionNode function)
    {
        Function = function;
    }
    
    public ICreateTransientScopeFunctionNode Function { get; }
}