using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.TaskCollection;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class DependencyA : ITaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyB : IValueTaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyC : ITypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

internal class DependencyD : IInterface
{
    public bool IsInitialized => true;
}

[MultiContainer(typeof(IReadOnlyList<Task<IInterface>>))]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = container.Create0();
        Assert.Equal(4, instance.Count);
        await Task.WhenAll(instance).ConfigureAwait(false);
        foreach (var task in instance)
            Assert.True((await task.ConfigureAwait(false)).IsInitialized);
    }
}