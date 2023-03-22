using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.OneExplicitConstructor;

internal class Inner {}

internal struct Dependency
{
    public Inner Inner { get; }
    
    internal Dependency(Inner inner) => Inner = inner;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
        Assert.NotNull(value.Inner);
    }
}