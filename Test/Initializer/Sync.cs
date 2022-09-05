using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Initializer.Sync;

internal class Dependency : IInitializer
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize() => IsInitialized = true;
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
        var instance = container.Create();
        Assert.True(instance.IsInitialized);
    }
}