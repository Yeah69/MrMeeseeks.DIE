using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryField.InContainerValueTask;

internal class Wrapper
{
    public string Property { get; }

    internal Wrapper(string property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private readonly ValueTask<string> DIE_Factory_Yeah = new ("Yeah");
}

public class Tests
{
    
    [Fact]
    public async Task Test()
    {
        await using var container = new Container();
        var wrapper = await container.Create().ConfigureAwait(false);
        Assert.Equal("Yeah", wrapper.Property);
    }
}