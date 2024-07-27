using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.ExternalType;


[AssemblyImplementationsAggregation(typeof(Uri))]
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
        var url = "https://example.com/path";
        var uri = container.Create()(url);
        Assert.NotNull(uri);
        Assert.IsType<Uri>(uri);
        Assert.Equal(url, uri.OriginalString);
    }
}