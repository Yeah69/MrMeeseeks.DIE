using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
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
    
    // ReSharper disable once InconsistentNaming
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
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        await container.DisposeAsync();
        Assert.True(instance.IsDisposed);
    }
}