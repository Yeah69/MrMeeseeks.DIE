using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.SingleWithChoice;

internal interface IInterface {}

internal class Class<T0> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<>), "T0", typeof(int), typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var list = container.Create();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.GetType() == typeof(Class<int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<string>));
    }
}