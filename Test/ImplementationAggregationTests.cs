using System;
using System.IO;
using MrMeeseeks.DIE;
using Xunit;

[assembly:ImplementationAggregation(typeof(FileInfo))]

namespace MrMeeseeks.DIE.Test;

internal partial class ImplementationAggregationContainer : IContainer<Func<string, FileInfo>>
{
    
}

public partial class ConstructorChoiceTests
{
    [Fact]
    public void ResolveExternalType()
    {
        using var container = new ImplementationAggregationContainer();
        var path = @"C:\HelloWorld.txt";
        var fileInfo = ((IContainer<Func<string, FileInfo>>) container).Resolve()(path);
        Assert.NotNull(fileInfo);
        Assert.IsType<FileInfo>(fileInfo);
        Assert.Equal(path, fileInfo.FullName);
    }
}