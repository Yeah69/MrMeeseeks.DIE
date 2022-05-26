using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.TypeInitializer.AsyncValueTask;

internal class Dependency : IValueTaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        IsInitialized = true;
        return ValueTask.CompletedTask;
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
        var instance = await container.CreateValueAsync().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}