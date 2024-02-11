using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.VanillaWithoutChoice;

internal abstract class Class {}

internal class SubClass : Class {}

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClass>(instance);
    }
}