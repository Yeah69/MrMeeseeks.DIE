using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Awaited.ContainerInstanceFunctionAsTask;


internal class Dependency : ITaskInitializer, IContainerInstance
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
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
    public async Task Test()
    {
        await using var container = new Container();
        var instance = container.CreateValueAsync();
        Assert.True((await instance.ConfigureAwait(false)).IsInitialized);
    }
}