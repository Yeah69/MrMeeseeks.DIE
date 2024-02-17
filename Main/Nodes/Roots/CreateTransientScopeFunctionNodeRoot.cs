using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ICreateTransientScopeFunctionNodeRoot
{
    ICreateTransientScopeFunctionNode Function { get; }
}

internal sealed class CreateTransientScopeFunctionNodeRoot : ICreateTransientScopeFunctionNodeRoot, IScopeRoot
{
    public CreateTransientScopeFunctionNodeRoot(ICreateTransientScopeFunctionNode function)
    {
        Function = function;
    }
    
    public ICreateTransientScopeFunctionNode Function { get; }
}