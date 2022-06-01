using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.ContainerUserDefinedAddForDisposal;

internal class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container
{
    private Dependency DIE_Factory_Dependency
    {
        get
        {
            var dependency = new Dependency();
            DIE_AddForDisposal(dependency);
            return dependency;
        }
    }

    private partial void DIE_AddForDisposal(IDisposable disposable);
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.True(instance.IsDisposed);
    }
}