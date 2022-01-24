using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Configuration;
using Xunit;

[assembly:ImplementationAggregation(typeof(DateTime))]
[assembly:ConstructorChoice(typeof(DateTime))]

namespace MrMeeseeks.DIE.Test;

internal partial class ConstructorChoiceContainer : IContainer<DateTime>
{
    
}

public partial class ImplementationAggregationTests
{
    [Fact]
    public void ResolveExternalType()
    {
        using var container = new ConstructorChoiceContainer();
        var dateTime = ((IContainer<DateTime>) container).Resolve();
        Assert.Equal(DateTime.MinValue, dateTime);
    }
}