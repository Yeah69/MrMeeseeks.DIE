using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.Interface.Vanilla;

internal interface IInterface;

internal class SubClassA : IInterface;

internal class SubClassB : IInterface;

[ImplementationChoice(typeof(IInterface), typeof(SubClassA))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}