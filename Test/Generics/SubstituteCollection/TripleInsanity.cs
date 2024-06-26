﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection.TripleInsanity;

internal interface IInterface;

// ReSharper disable UnusedTypeParameter
internal sealed class Class<T0, T1, T2> : IInterface;
// ReSharper restore UnusedTypeParameter

[GenericParameterSubstitutesChoice(typeof(Class<,,>), "T0", typeof(int), typeof(string), typeof(uint), typeof(bool),
    typeof(byte))]
[GenericParameterSubstitutesChoice(typeof(Class<,,>), "T1", typeof(int), typeof(string), typeof(uint), typeof(bool),
    typeof(byte))]
[GenericParameterSubstitutesChoice(typeof(Class<,,>), "T2", typeof(int), typeof(string), typeof(uint), typeof(bool),
    typeof(byte))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var list = container.Create();
        Assert.Equal(125, list.Count);
    }
}