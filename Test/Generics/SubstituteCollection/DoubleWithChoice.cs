﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.DoubleWithChoice;

// ReSharper disable once UnusedTypeParameter
internal interface IInterface<T0>;

// ReSharper disable once UnusedTypeParameter
internal sealed class Class<T0, T1> : IInterface<T0>;

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(int))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IReadOnlyList<IInterface<int>>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var list = container.Create();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i is Class<int, int>);
        Assert.Contains(list, i => i is Class<int, string>);
    }
}