using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ConstructorChoice.Parameterless;

[ImplementationAggregation(typeof(DateTime))]
[ConstructorChoice(typeof(DateTime))]
[CreateFunction(typeof(DateTime), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var dateTime = container.Create();
        Assert.Equal(DateTime.MinValue, dateTime);
    }
}