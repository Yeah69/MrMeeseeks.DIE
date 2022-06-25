using System.IO;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FactoryWithParameterInContainer;

[ImplementationAggregation(typeof(FileInfo))]
[CreateFunction(typeof(FileInfo), "Create")]
internal sealed partial class Container
{
    private string DIE_Factory_Path => "C:\\Yeah.txt";
    private FileInfo DIE_Factory(string path) => new (path);
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var fileInfo = container.Create();
        Assert.Equal("C:\\Yeah.txt", fileInfo.FullName);
    }
}