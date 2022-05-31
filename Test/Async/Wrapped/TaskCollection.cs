using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.TaskCollection;

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

[CreateFunction(typeof(IReadOnlyList<Task<IInterface>>), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.Equal(4, instance.Count);
        await Task.WhenAll(instance).ConfigureAwait(false);
        foreach (var task in instance)
            Assert.True((await task.ConfigureAwait(false)).IsInitialized);
    }
}