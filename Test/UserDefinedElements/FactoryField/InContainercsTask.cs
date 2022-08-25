using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryField.InContainerTask;

internal class Wrapper
{
    public Task<string> Property { get; }

    internal Wrapper(Task<string> property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private readonly Task<string> DIE_Factory_Yeah = Task.FromResult("Yeah");
}

public class Tests
{
    
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var wrapper = container.Create();
        Assert.Equal("Yeah", await wrapper.Property.ConfigureAwait(false));
    }
}