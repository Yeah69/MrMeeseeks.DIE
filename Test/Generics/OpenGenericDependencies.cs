using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericDependencies;

internal record struct Dependency<T0>(T0 _);

internal sealed class DependencyHolder<T0>
{
    public Dependency<T0> Dependency { get; set; }
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(Dependency<T0> _) {}
}

[PropertyChoice(typeof(DependencyHolder<>), nameof(DependencyHolder<int>.Dependency))]
[CreateFunction(typeof(DependencyHolder<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyHolder<int>>(instance);
    }
}