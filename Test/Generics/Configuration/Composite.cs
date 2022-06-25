using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

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
    
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var composite = container.Create();
        Assert.IsType<Composite<int>>(composite);
    }
}