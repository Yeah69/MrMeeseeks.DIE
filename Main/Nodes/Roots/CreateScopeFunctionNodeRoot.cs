using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ICreateScopeFunctionNodeRoot
{
    ICreateScopeFunctionNode Function { get; }
}

internal class CreateScopeFunctionNodeRoot(ICreateScopeFunctionNode function) : ICreateScopeFunctionNodeRoot, IScopeRoot
{
    public ICreateScopeFunctionNode Function { get; } = function;
}