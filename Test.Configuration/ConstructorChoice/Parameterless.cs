using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ConstructorChoice.Parameterless;

[ImplementationAggregation(typeof(DateTime))]
[ConstructorChoice(typeof(DateTime))]
[CreateFunction(typeof(DateTime), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var dateTime = container.Create();
        Assert.Equal(DateTime.MinValue, dateTime);
    }
}