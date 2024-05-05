using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.SyncOnly.InScope;

internal sealed class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose() => IsDisposed = true;
}

internal sealed class ScopeRoot : IScopeRoot
{
    public ScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = Container.DIE_CreateContainer();
        var dependency = container.Create();
        Assert.False(dependency.Dependency.IsDisposed);
        container.Dispose();
        Assert.True(dependency.Dependency.IsDisposed);
    }
}