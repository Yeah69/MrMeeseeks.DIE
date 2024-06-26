using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryProperty.InContainerTask;

internal sealed class Wrapper
{
    public Task<string> Property { get; }

    internal Wrapper(Task<string> property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    // ReSharper disable once InconsistentNaming
    private Task<string> DIE_Factory_Yeah => Task.FromResult("Yeah");
    
    
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