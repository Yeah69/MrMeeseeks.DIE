using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

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
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}