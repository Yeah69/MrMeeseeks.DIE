﻿using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Collection.Composite.IReadOnlyCollection;

internal interface IInterface;

internal class ClassA : IInterface;

internal class ClassB : IInterface;

internal class ClassC : IInterface;

internal class Composite : IInterface, IComposite<IInterface>
{
    // ReSharper disable once UnusedParameter.Local
    internal Composite(IReadOnlyCollection<IInterface> _) {}
}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}