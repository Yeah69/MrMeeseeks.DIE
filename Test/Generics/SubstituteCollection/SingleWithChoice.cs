using System;
using System.Collections.Generic;
using System.Linq;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.Single;

internal interface IInterface {}

internal class Class<T0> : IInterface {}

[GenericParameterSubstituteAggregation(typeof(Class<>), "T0", typeof(int))]
[GenericParameterChoice(typeof(Class<>), "T0", typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var list = container.Create();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.GetType() == typeof(Class<int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<string>));
    }
}