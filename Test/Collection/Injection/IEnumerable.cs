using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Collection.Injection.IEnumerable;

internal interface IInterface;
internal sealed class ClassA : IInterface;

internal sealed class ClassB : IInterface;

internal sealed class ClassC : IInterface;

[CreateFunction(typeof(IEnumerable<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}