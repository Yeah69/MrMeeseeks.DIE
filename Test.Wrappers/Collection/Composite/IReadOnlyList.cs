using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.Test.Wrappers.Collection.Composite.IReadOnlyList;

internal interface IInterface;

internal sealed class ClassA : IInterface;

internal sealed class ClassB : IInterface;

internal sealed class ClassC : IInterface;

internal sealed class Composite : IInterface, IComposite<IInterface>
{
    // ReSharper disable once UnusedParameter.Local
    internal Composite(IReadOnlyList<IInterface> _) {}
}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        _ = container.Create();
    }
}