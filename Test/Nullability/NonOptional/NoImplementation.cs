using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Nullability.NonOptional.NoImplementation;

internal interface IDependency{}

internal class Wrapper
{
    public IDependency? Dependency { get; }

    internal Wrapper(IDependency? dependency) => Dependency = dependency;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var dependency = container.Create().Dependency;
        Assert.Null(dependency);
    }
}