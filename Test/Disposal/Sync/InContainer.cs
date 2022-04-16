using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Sync.InContainer;

internal class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        IsDisposed = true;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
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
        Assert.False(dependency.IsDisposed);
        container.Dispose();
        Assert.True(dependency.IsDisposed);
    }
}