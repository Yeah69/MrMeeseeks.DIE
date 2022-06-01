using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FactoryInContainer;

internal class Wrapper
{
    public string Property { get; }

    internal Wrapper(string property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal partial class Container
{
    private string DIE_Factory_Yeah() => "Yeah";
}

public class Tests
{
    
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var wrapper = container.Create();
        Assert.Equal("Yeah", wrapper.Property);
    }
}