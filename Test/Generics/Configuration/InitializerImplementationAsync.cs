using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.InitializerImplementationAsync;

// ReSharper disable once UnusedTypeParameter
internal sealed class Dependency<T0>
{
    internal async Task InitializeAsync()
    {
        await Task.Delay(500);
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }
}

[Initializer(typeof(Dependency<>), nameof(Dependency<int>.InitializeAsync))]
[CreateFunction(typeof(Dependency<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create();
        Assert.True(instance.IsInitialized);
    }
}