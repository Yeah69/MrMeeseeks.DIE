using System;
using System.IO;
using MrMeeseeks.DIE.Configuration;
using Xunit;

[assembly:ImplementationAggregation(typeof(FileInfo))]

namespace MrMeeseeks.DIE.Test;

[CreateFunction(typeof(Func<string, FileInfo>), "Create")]
internal partial class ImplementationAggregationContainer
{
    
}

public partial class ConstructorChoiceTests
{
    [Fact]
    public void ResolveExternalType()
    {
        var container = new ImplementationAggregationContainer();
        var path = @"C:\HelloWorld.txt";
        var fileInfo = container.Create()(path);
        Assert.NotNull(fileInfo);
        Assert.IsType<FileInfo>(fileInfo);
        Assert.Equal(path, fileInfo.FullName);
    }
}