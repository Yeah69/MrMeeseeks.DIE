using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface ICreateFunctionNodeRoot
{
    ICreateFunctionNode Function { get; }
}

internal class CreateFunctionNodeRoot : ICreateFunctionNodeRoot, IScopeRoot
{
    public CreateFunctionNodeRoot(ICreateFunctionNode function)
    {
        Function = function;
    }
    
    public ICreateFunctionNode Function { get; }
}