using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.TypeInitializer.AsyncTask;

internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    Task ITaskTypeInitializer.InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container 
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        var container = new Container();
        var instance = await container.CreateAsync().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}