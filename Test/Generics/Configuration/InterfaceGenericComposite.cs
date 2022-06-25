using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.InterfaceGenericComposite;

internal interface IInterface<T0>
{
    IReadOnlyList<IInterface<T0>> Implementations { get; }
}

internal class BaseA<T0> : IInterface<T0>
{
    public IReadOnlyList<IInterface<T0>> Implementations => new[] { this };
}

internal class BaseB<T0> : IInterface<T0>
{
    public IReadOnlyList<IInterface<T0>> Implementations => new[] { this };
}

internal class Composite<T0, T1> : IInterface<T0>, IComposite<IInterface<T0>>
{
    public IReadOnlyList<IInterface<T0>> Implementations { get; }

    internal Composite(
        IReadOnlyList<IInterface<T0>> implementations) =>
        Implementations = implementations;
}

[GenericParameterChoice(typeof(Composite<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var composite = container.Create();
        Assert.IsType<Composite<int, string>>(composite);
    }
}