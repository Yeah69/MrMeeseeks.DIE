using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Functions;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface ILocalFunctionNodeRoot
{
    ILocalFunctionNode Function { get; }
}

internal class LocalFunctionNodeRoot(ILocalFunctionNode function) : ILocalFunctionNodeRoot, IScopeRoot
{
    public ILocalFunctionNode Function { get; } = function;
}