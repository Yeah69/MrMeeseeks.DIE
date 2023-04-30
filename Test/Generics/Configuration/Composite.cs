using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.Composite;

internal interface IInterface
{
    IReadOnlyList<IInterface> Implementations { get; }
}

internal class BaseA : IInterface
{
    public IReadOnlyList<IInterface> Implementations => new[] { this };
}

internal class BaseB : IInterface
{
    public IReadOnlyList<IInterface> Implementations => new[] { this };
}

// ReSharper disable once UnusedTypeParameter
internal class Composite<T0> : IInterface, IComposite<IInterface>
{
    public IReadOnlyList<IInterface> Implementations { get; }

    internal Composite(
        IReadOnlyList<IInterface> implementations) =>
        Implementations = implementations;
}

[GenericParameterChoice(typeof(Composite<>), "T0", typeof(int))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var composite = container.Create();
        Assert.IsType<Composite<int>>(composite);
    }
}