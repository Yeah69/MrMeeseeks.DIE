using System;
using System.IO;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.ImplementationAggregation.ExternalType;


[ImplementationAggregation(typeof(FileInfo))]
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
        var path = @"C:\HelloWorld.txt";
        var fileInfo = container.Create()(path);
        Assert.NotNull(fileInfo);
        Assert.IsType<FileInfo>(fileInfo);
        Assert.Equal(path, fileInfo.FullName);
    }
}