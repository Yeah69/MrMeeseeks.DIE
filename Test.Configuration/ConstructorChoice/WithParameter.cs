using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ConstructorChoice.WithParameter;

[ImplementationAggregation(typeof(Uri))]
[ConstructorChoice(typeof(Uri), typeof(string))]
[CreateFunction(typeof(Func<string, Uri>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var uri = container.Create()("https://example.com/path");
        Assert.Equal("https://example.com/path", uri.OriginalString);
    }
}