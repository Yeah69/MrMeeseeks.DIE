using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ICreateFunctionNodeRoot
{
    ICreateFunctionNode Function { get; }
}

internal sealed class CreateFunctionNodeRoot : ICreateFunctionNodeRoot, IScopeRoot
{
    public CreateFunctionNodeRoot(ICreateFunctionNode function)
    {
        Function = function;
    }
    
    public ICreateFunctionNode Function { get; }
}