using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.InContainerTask;

internal class Wrapper
{
    public Task<string> Property { get; }

    internal Wrapper(Task<string> property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private Task<string> DIE_Factory_Yeah() => Task.FromResult("Yeah");
    
    private Container() {}
}

public class Tests
{
    
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var wrapper = container.Create();
        Assert.Equal("Yeah", await wrapper.Property.ConfigureAwait(false));
    }
}