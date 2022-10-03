using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Collection.Composite.Array;

internal interface IInterface {}

internal class ClassA : IInterface {}

internal class ClassB : IInterface {}

internal class ClassC : IInterface {}

internal class Composite : IInterface, IComposite<IInterface>
{
    internal Composite(IInterface[] _) {}
}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var _ = container.Create();
    }
}