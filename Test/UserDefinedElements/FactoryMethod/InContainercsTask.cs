using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.InContainerTask;

internal sealed class Wrapper
{
    public Task<string> Property { get; }

    internal Wrapper(Task<string> property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private async Task<string> DIE_Factory_Yeah()
    {
        await Task.Yield();
        return "Yeah";
    }

    
}

public sealed class Tests
{
    
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var wrapper = container.Create();
        Assert.Equal("Yeah", await wrapper.Property);
    }
}