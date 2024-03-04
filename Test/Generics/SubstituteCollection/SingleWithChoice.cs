using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.Single;

internal interface IInterface;

// ReSharper disable once UnusedTypeParameter
internal class Class<T0> : IInterface;

[GenericParameterSubstitutesChoice(typeof(Class<>), "T0", typeof(int))]
[GenericParameterChoice(typeof(Class<>), "T0", typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var list = container.Create();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.GetType() == typeof(Class<int>));
        Assert.Contains(list, i => i.GetType() == typeof(Class<string>));
    }
}