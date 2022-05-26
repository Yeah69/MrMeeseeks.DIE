using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FieldInContainer;

internal class Wrapper
{
    public string Property { get; }

    internal Wrapper(string property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal partial class Container
{
    private readonly string DIE_Yeah = "Yeah";
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