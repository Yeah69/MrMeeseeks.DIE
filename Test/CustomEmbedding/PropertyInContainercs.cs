using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.PropertyInContainer;

internal class Wrapper
{
    public string Property { get; }

    internal Wrapper(string property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal partial class Container
{
    private string DIE_Yeah => "Yeah";
}

public class Tests
{
    
    [Fact]
    public void Test()
    {
        var container = new Container();
        var wrapper = container.Create();
        Assert.Equal("Yeah", wrapper.Property);
    }
}