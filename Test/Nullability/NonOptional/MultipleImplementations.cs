using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Nullability.NonOptional.MultipleImplementations;

internal interface IDependency{}

internal class Dependency0 : IDependency {}

internal class Dependency1 : IDependency {}

internal class Wrapper
{
    public IDependency? Dependency { get; }

    internal Wrapper(IDependency? dependency) => Dependency = dependency;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var dependency = container.Create().Dependency;
        Assert.Null(dependency);
    }
}