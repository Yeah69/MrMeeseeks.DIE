using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ICreateTransientScopeFunctionNodeRoot
{
    ICreateTransientScopeFunctionNode Function { get; }
}

internal class CreateTransientScopeFunctionNodeRoot
    (ICreateTransientScopeFunctionNode function) : ICreateTransientScopeFunctionNodeRoot, IScopeRoot
{
    public ICreateTransientScopeFunctionNode Function { get; } = function;
}