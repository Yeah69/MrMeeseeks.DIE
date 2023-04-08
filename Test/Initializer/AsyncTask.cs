using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Initializer.AsyncTask;

internal class Dependency : ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    Task ITaskInitializer.InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
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
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}