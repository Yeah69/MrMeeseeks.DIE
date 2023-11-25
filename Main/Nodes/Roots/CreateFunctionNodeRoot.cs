using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ICreateFunctionNodeRoot
{
    ICreateFunctionNode Function { get; }
}

internal class CreateFunctionNodeRoot(ICreateFunctionNode function) : ICreateFunctionNodeRoot, IScopeRoot
{
    public ICreateFunctionNode Function { get; } = function;
}