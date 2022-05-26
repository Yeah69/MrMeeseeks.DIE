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
internal partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
        Assert.NotNull(value.Inner);
    }
}