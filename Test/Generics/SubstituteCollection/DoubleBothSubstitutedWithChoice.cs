using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.BothSubstitutedWithChoice;

internal interface IInterface {}

internal class Class<T0, T1> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T0", typeof(bool))]
[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(int))]
[GenericParameterChoice(typeof(Class<,>), "T0", typeof(byte))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var list = container.Create();
        Assert.Equal(4, list.Count);
        Assert.Contains(list, i => i.GetType() == typeof(Class<bool, int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<bool, string>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<byte, int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<byte, string>));
    }
}