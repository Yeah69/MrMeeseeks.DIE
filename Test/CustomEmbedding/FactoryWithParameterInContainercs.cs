using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FactoryWithParameterInContainer;

[ImplementationAggregation(typeof(FileInfo))]
[CreateFunction(typeof(FileInfo), "Create")]
internal partial class Container
{
    private string DIE_Path => "C:\\Yeah.txt";
    private FileInfo DIE_Factory(string path) => new (path);
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var fileInfo = container.Create();
        Assert.Equal("C:\\Yeah.txt", fileInfo.FullName);
    }
}