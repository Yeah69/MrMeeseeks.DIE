using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Collection.Composite.IEnumerable;

internal interface IInterface {}

internal class ClassA : IInterface {}

internal class ClassB : IInterface {}

internal class ClassC : IInterface {}

internal class Composite : IInterface, IComposite<IInterface>
{
    internal Composite(IEnumerable<IInterface> _) {}
}

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
        var _ = container.Create();
    }
}