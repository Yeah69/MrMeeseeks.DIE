using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Collection.Composite.IReadOnlyList;

internal interface IInterface {}

internal class ClassA : IInterface {}

internal class ClassB : IInterface {}

internal class ClassC : IInterface {}

internal class Composite : IInterface, IComposite<IInterface>
{
    internal Composite(IReadOnlyList<IInterface> _) {}
}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var _ = container.Create();
    }
}