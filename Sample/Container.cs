using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface {}

internal class Dependency : ITransientScopeInstance, IInitializer, IInterface, IDisposable
{
    void IInitializer.Initialize()
    {
    }

    public void Dispose()
    {
        Console.WriteLine("sync");
    }
}

internal class SyncDependency : IInterface
{
}

internal class OuterDependency : ITransientScopeInstance
{
    internal OuterDependency(IInterface _) {}
}

internal class Root : ITransientScopeRoot
{
    private readonly IAsyncDisposable _disposable;

    internal Root(
        ValueTask<OuterDependency> ___, 
        Func<Dependency, ValueTask<OuterDependency>> __,
        Func<ValueTask<OuterDependency>> _,
        IAsyncDisposable disposable)
    {
        _disposable = disposable;
    }

    public void Clean() => _disposable.DisposeAsync();
}

[ImplementationChoice(typeof(IInterface), typeof(SyncDependency))]
[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
    [ImplementationChoice(typeof(IInterface), typeof(Dependency))]
    private partial class DIE_DefaultTransientScope {}
}