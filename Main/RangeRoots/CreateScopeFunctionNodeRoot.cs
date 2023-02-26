using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface ICreateScopeFunctionNodeRoot
{
    ICreateScopeFunctionNode Function { get; }
}

internal class CreateScopeFunctionNodeRoot : ICreateScopeFunctionNodeRoot, IScopeRoot
{
    public CreateScopeFunctionNodeRoot(ICreateScopeFunctionNode function)
    {
        Function = function;
    }
    
    public ICreateScopeFunctionNode Function { get; }
}