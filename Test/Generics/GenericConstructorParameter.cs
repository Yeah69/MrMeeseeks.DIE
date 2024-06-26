

using System.Threading.Tasks;
using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.GenericConstructorParameter;

internal sealed record Dependency;

internal sealed class DependencyHolder<T0>
{
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(T0 _) {}
}

[CreateFunction(typeof(DependencyHolder<Dependency>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyHolder<Dependency>>(instance);
    }
}