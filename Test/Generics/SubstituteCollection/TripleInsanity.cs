using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.TripleInsanity;

internal interface IInterface {}

internal class Class<T0, T1, T2> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<,,>), "T0", typeof(int), typeof(string), typeof(uint), typeof(bool), typeof(byte))]
[GenericParameterSubstitutesChoice(typeof(Class<,,>), "T1", typeof(int), typeof(string), typeof(uint), typeof(bool), typeof(byte))]
[GenericParameterSubstitutesChoice(typeof(Class<,,>), "T2", typeof(int), typeof(string), typeof(uint), typeof(bool), typeof(byte))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var list = container.Create();
        Assert.Equal(125, list.Count);
    }
}