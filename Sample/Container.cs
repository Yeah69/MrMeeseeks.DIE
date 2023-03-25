using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency
{
    internal bool IsDisposed => true;
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    internal Dependency Dependency { get; }
    private readonly IDisposable _disposable;

    internal TransientScopeRoot(
        Dependency dependency,
        IDisposable disposable)
    {
        Dependency = dependency;
        _disposable = disposable;
    }

    internal void Cleanup() => _disposable.Dispose();
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    private Container() {}
}