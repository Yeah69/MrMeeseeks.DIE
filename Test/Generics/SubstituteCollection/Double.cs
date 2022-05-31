using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.Double;

internal interface IInterface<T0> {}

internal class Class<T0, T1> : IInterface<T0> {}

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(int), typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface<int>>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var list = container.Create();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.GetType() == typeof(Class<int, int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<int, string>));
    }
}