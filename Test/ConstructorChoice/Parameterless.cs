using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.ConstructorChoice.Parameterless;

[ImplementationAggregation(typeof(DateTime))]
[ConstructorChoice(typeof(DateTime))]
[CreateFunction(typeof(DateTime), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var dateTime = container.Create();
        Assert.Equal(DateTime.MinValue, dateTime);
    }
}