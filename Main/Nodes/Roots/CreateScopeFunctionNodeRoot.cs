using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ICreateScopeFunctionNodeRoot
{
    ICreateScopeFunctionNode Function { get; }
}

internal sealed class CreateScopeFunctionNodeRoot : ICreateScopeFunctionNodeRoot, IScopeRoot
{
    public CreateScopeFunctionNodeRoot(ICreateScopeFunctionNode function)
    {
        Function = function;
    }
    
    public ICreateScopeFunctionNode Function { get; }
}