using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InTransientScope;

internal interface IInterface
{
    bool IsDisposed { get; }
}

internal class Dependency : IInterface, ITransientScopeInstance, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal class TransientScope : ITransientScopeRoot
{
    private readonly IDisposable _scopeDisposal;
    public IInterface Dependency { get; }
    public TransientScope(IDisposable scopeDisposal, IInterface dependency)
    {
        _scopeDisposal = scopeDisposal;
        Dependency = dependency;
    }

    public void Cleanup() => _scopeDisposal.Dispose();
}

[CreateFunction(typeof(TransientScope), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.IsType<Dependency>(transientScopeRoot.Dependency);
        Assert.False(transientScopeRoot.Dependency.IsDisposed);
        transientScopeRoot.Cleanup();
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}