using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.Interface.CollectionWithoutChoice;

internal interface IInterface;

internal sealed class SubClassA : IInterface;

internal sealed class SubClassB : IInterface;

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
    }
}