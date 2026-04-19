using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.WithParameterInContainer;

[FilterAllImplementationsAggregation]
[ImplementationAggregation(typeof(Uri))]
[CreateFunction(typeof(Uri), "Create")]
internal sealed partial class Container
{
    // ReSharper disable once InconsistentNaming
    private string DIE_Factory_Url => "https://example.com/path";

    private Uri DIE_Factory(string url) => new (url);
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var uri = container.Create();
        Assert.Equal("https://example.com/path", uri.OriginalString);
    }
}