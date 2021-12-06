using System;
using System.IO;
using MrMeeseeks.DIE;
using Xunit;

[assembly:ImplementationAggregation(typeof(FileInfo))]

namespace MrMeeseeks.DIE.Sample;

internal partial class ImplementationAggregationContainer : IContainer<Func<string, FileInfo>>
{
    
}

public partial class ImplementationAggregationTests
{
    [Fact]
    public void ResolveExternalType()
    {
        using var container = new ImplementationAggregationContainer();
        var fileInfo = ((IContainer<Func<string, FileInfo>>) container).Resolve()(@"C:\HelloWorld.txt");
        Assert.NotNull(fileInfo);
        Assert.IsType<FileInfo>(fileInfo);
    }
}