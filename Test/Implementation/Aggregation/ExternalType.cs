using System;
using System.IO;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.ExternalType;


[AssemblyImplementationsAggregation(typeof(FileInfo))]
[ImplementationAggregation(typeof(FileInfo))]
[CreateFunction(typeof(Func<string, FileInfo>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var path = @"C:\HelloWorld.txt";
        var fileInfo = container.Create()(path);
        Assert.NotNull(fileInfo);
        Assert.IsType<FileInfo>(fileInfo);
        Assert.Equal(path, fileInfo.FullName);
    }
}