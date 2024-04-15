using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Implementation.Double;

// ReSharper disable UnusedTypeParameter
internal sealed class Class<T0, T1>;
// ReSharper restore UnusedTypeParameter

[CreateFunction(typeof(Class<int, string>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}