using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.ArrayType;

internal interface IInterface;

internal sealed class DependencyA : IInterface;

internal sealed class DependencyB : IInterface;

internal sealed class DependencyC : IInterface;

[CreateFunction(typeof(IInterface[]), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.Equal(3, instance.Length);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
    }
}