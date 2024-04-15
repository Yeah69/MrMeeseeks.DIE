using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.BothSubstitutedWithChoice;

internal interface IInterface;

// ReSharper disable UnusedTypeParameter
internal sealed class Class<T0, T1> : IInterface;
// ReSharper restore UnusedTypeParameter

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T0", typeof(bool))]
[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(int))]
[GenericParameterChoice(typeof(Class<,>), "T0", typeof(byte))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var list = container.Create();
        Assert.Equal(4, list.Count);
        Assert.Contains(list, i => i is Class<bool, int>);
        Assert.Contains(list, i => i is Class<bool, string>);
        Assert.Contains(list, i => i is Class<byte, int>);
        Assert.Contains(list, i => i is Class<byte, string>);
    }
}