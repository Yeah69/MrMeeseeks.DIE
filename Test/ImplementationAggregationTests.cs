using System;
using System.IO;
using MrMeeseeks.DIE.Configuration;
using Xunit;

[assembly:ImplementationAggregation(typeof(FileInfo))]

namespace MrMeeseeks.DIE.Test;

[MultiContainer(typeof(Func<string, FileInfo>))]
internal partial class ImplementationAggregationContainer
{
    
}

public partial class ConstructorChoiceTests
{
    [Fact]
    public void ResolveExternalType()
    {
        using var container = new ImplementationAggregationContainer();
        var path = @"C:\HelloWorld.txt";
        var fileInfo = container.Create0()(path);
        Assert.NotNull(fileInfo);
        Assert.IsType<FileInfo>(fileInfo);
        Assert.Equal(path, fileInfo.FullName);
    }
}