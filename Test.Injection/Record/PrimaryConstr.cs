using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Record.PrimaryConstr;

internal sealed record Dependency;

internal sealed record Implementation(Dependency Dependency);

[CreateFunction(typeof(Implementation), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Implementation>(instance);
        Assert.IsType<Dependency>(instance.Dependency);
    }
}