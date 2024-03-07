using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.WithParameterInContainer;

[FilterAllImplementationsAggregation]
[ImplementationAggregation(typeof(FileInfo))]
[CreateFunction(typeof(FileInfo), "Create")]
internal sealed partial class Container
{
    // ReSharper disable once InconsistentNaming
    private string DIE_Factory_Path => "C:\\Yeah.txt";
    
    private FileInfo DIE_Factory(string path) => new (path);
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var fileInfo = container.Create();
        Assert.Equal("C:\\Yeah.txt", fileInfo.FullName);
    }
}