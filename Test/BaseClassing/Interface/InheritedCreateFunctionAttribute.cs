using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Interface.InheritedCreateFunctionAttribute;

internal sealed class Class;

[CreateFunction(typeof(Class), "Create")]
internal interface IContainerBase;

internal sealed partial class Container : IContainerBase;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.NotNull(instance);
    }
}