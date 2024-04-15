using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.InContainer;

internal sealed class Wrapper
{
    public string Property { get; }

    internal Wrapper(string property) => Property = property;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private string DIE_Factory_Yeah() => "Yeah";
    
    
}

public sealed class Tests
{
    
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var wrapper = container.Create();
        Assert.Equal("Yeah", wrapper.Property);
    }
}