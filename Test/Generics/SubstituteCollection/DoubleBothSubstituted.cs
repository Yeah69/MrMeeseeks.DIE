using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.BothSubstituted;

internal interface IInterface {}

internal class Class<T0, T1> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T0", typeof(bool), typeof(byte))]
[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(int), typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var list = container.Create();
        Assert.Equal(4, list.Count);
        Assert.Contains(list, i => i.GetType() == typeof(Class<bool, int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<bool, string>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<byte, int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<byte, string>));
    }
}