using System.Threading.Tasks;
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
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
        Assert.NotNull(value.Inner);
    }
}