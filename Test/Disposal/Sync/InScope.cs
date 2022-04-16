using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Sync.InScope;

internal class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose() => IsDisposed = true;
}

internal class ScopeRoot : IScopeRoot
{
    public ScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var dependency = container.Create();
        Assert.False(dependency.Dependency.IsDisposed);
        container.Dispose();
        Assert.True(dependency.Dependency.IsDisposed);
    }
}