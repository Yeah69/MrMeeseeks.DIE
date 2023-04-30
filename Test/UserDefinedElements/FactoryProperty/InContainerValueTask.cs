using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryProperty.InContainerValueTask;

internal class Wrapper
{
    public string Property { get; }

    internal Wrapper(string property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    // ReSharper disable once InconsistentNaming
    private ValueTask<string> DIE_Factory_Yeah => new ("Yeah");
    
    private Container() {}
}

public class Tests
{
    
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var wrapper = await container.Create().ConfigureAwait(false);
        Assert.Equal("Yeah", wrapper.Property);
    }
}