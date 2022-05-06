using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.InitializerImplementationSync;

internal class Dependency<T0>
{
    internal void Initialize()
    {
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }
}

[TypeInitializer(typeof(Dependency<>), nameof(Dependency<int>.Initialize))]
[CreateFunction(typeof(Dependency<int>), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.True(instance.IsInitialized);
    }
}