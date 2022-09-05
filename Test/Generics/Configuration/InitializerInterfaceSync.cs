using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.InitializerInterfaceSync;

internal class Dependency<T0> : IInitializer
{
    void IInitializer.Initialize()
    {
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }
}

[CreateFunction(typeof(Dependency<int>), "Create")]
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