using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.Interface.Vanilla;

internal interface IInterface;

internal sealed class SubClassA : IInterface;

internal class SubClassB : IInterface;

[ImplementationChoice(typeof(IInterface), typeof(SubClassA))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}