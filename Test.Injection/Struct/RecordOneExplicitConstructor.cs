using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Struct.RecordOneExplicitConstructor;

internal sealed class Inner;

internal record struct Dependency(Inner Inner);

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
        Assert.NotNull(value.Inner);
    }
}