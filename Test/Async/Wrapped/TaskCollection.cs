using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.TaskCollection;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class DependencyA : ITaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyB : IValueTaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyC : IInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

internal class DependencyD : IInterface
{
    public bool IsInitialized => true;
}

[CreateFunction(typeof(IReadOnlyList<Task<IInterface>>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.Equal(4, instance.Count);
        await Task.WhenAll(instance).ConfigureAwait(false);
        foreach (var task in instance)
            Assert.True((await task.ConfigureAwait(false)).IsInitialized);
    }
}