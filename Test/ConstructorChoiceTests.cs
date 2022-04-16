using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

[assembly:ImplementationAggregation(typeof(DateTime))]
[assembly:ConstructorChoice(typeof(DateTime))]

namespace MrMeeseeks.DIE.Test;

[CreateFunction(typeof(DateTime), "Create")]
internal partial class ConstructorChoiceContainer
{
    
}

public class ImplementationAggregationTests
{
    [Fact]
    public void ResolveExternalType()
    {
        var container = new ConstructorChoiceContainer();
        var dateTime = container.Create();
        Assert.Equal(DateTime.MinValue, dateTime);
    }
}