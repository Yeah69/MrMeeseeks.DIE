using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.InterfaceGenericComposite;

internal interface IInterface<T0>
{
    IReadOnlyList<IInterface<T0>> Implementations { get; }
}

internal sealed class BaseA<T0> : IInterface<T0>
{
    public IReadOnlyList<IInterface<T0>> Implementations => new[] { this };
}

internal sealed class BaseB<T0> : IInterface<T0>
{
    public IReadOnlyList<IInterface<T0>> Implementations => new[] { this };
}

// ReSharper disable once UnusedTypeParameter
internal sealed class Composite<T0, T1> : IInterface<T0>, IComposite<IInterface<T0>>
{
    public IReadOnlyList<IInterface<T0>> Implementations { get; }

    internal Composite(
        IReadOnlyList<IInterface<T0>> implementations) =>
        Implementations = implementations;
}

[GenericParameterChoice(typeof(Composite<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var composite = container.Create();
        Assert.IsType<Composite<int, string>>(composite);
    }
}