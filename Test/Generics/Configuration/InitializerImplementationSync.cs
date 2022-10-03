using MrMeeseeks.DIE.Configuration.Attributes;
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

[Initializer(typeof(Dependency<>), nameof(Dependency<int>.Initialize))]
[CreateFunction(typeof(Dependency<int>), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.True(instance.IsInitialized);
    }
}