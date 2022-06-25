using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.ContainerUserDefinedAddForDisposalAsync;

internal class Dependency : IAsyncDisposable
{
    internal bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return new ValueTask(Task.CompletedTask);
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Dependency DIE_Factory_Dependency
    {
        get
        {
            var dependency = new Dependency();
            DIE_AddForDisposalAsync(dependency);
            return dependency;
        }
    }

    private partial void DIE_AddForDisposalAsync(IAsyncDisposable asyncDisposable);
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