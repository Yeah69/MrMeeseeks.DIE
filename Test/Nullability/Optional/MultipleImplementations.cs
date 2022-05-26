using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Nullability.Optional.MultipleImplementations;

internal interface IDependency{}

internal class Dependency0 : IDependency {}

internal class Dependency1 : IDependency {}

internal class Wrapper
{
    public IDependency? Dependency { get; }

    internal Wrapper(IDependency? dependency = null) => Dependency = dependency;
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