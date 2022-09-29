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
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var dependency = container.Create().Dependency;
        Assert.Null(dependency);
    }
}