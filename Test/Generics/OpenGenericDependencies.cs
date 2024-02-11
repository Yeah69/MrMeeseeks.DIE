using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericDependencies;

internal record struct Dependency<T0>(T0 _);

internal class DependencyHolder<T0>
{
    public Dependency<T0> Dependency { get; set; }
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(Dependency<T0> _) {}
}

[PropertyChoice(typeof(DependencyHolder<>), nameof(DependencyHolder<int>.Dependency))]
[CreateFunction(typeof(DependencyHolder<int>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyHolder<int>>(instance);
    }
}