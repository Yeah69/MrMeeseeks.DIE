using System;
using System.IO;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.ConstructorChoice.WithParameter;

[ImplementationAggregation(typeof(FileInfo))]
[ConstructorChoice(typeof(FileInfo), typeof(string))]
[CreateFunction(typeof(Func<string, FileInfo>), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var fileInfo = container.Create()("C:\\Yeah.txt");
        Assert.Equal("C:\\Yeah.txt", fileInfo.FullName);
    }
}