using System.Threading.Tasks;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.GenericDependenciesAsync;

internal interface IInterface<T0>;

internal sealed class Dependency<T0> : IInterface<T0>
{
    public async ValueTask Initialize() => await Task.Yield();
}

internal interface IInterface<T3, T4, T5>
{
    ValueTask<IInterface<T5>> DependencyInit { get; }
}

internal sealed class DependencyHolder<T0, T1, T2> : IInterface<T2, T1, T0>
{
    public required ValueTask<IInterface<T0>> DependencyInit { get; init; }
}

[Initializer(typeof(Dependency<>), nameof(Dependency<int>.Initialize))]
[CreateFunction(typeof(DependencyHolder<,,>), "Create")]
[CreateFunction(typeof(IInterface<,,>), "CreateInterface")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create<string, object, int>();
        Assert.IsType<DependencyHolder<string, object, int>>(instance);
        Assert.IsType<Dependency<string>>(await instance.DependencyInit);
        var interfaceInstance = container.CreateInterface<string, object, int>();
        Assert.IsType<DependencyHolder<int, object, string>>(interfaceInstance);
        Assert.IsType<Dependency<int>>(await interfaceInstance.DependencyInit);
    }
}