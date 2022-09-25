using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Initializer.AsyncValueTask;

internal class Dependency : IValueTaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Yield();
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container 
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = await container.CreateValueAsync().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}