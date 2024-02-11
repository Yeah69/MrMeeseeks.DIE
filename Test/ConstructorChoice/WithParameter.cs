using System;
using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ConstructorChoice.WithParameter;

[ImplementationAggregation(typeof(FileInfo))]
[ConstructorChoice(typeof(FileInfo), typeof(string))]
[CreateFunction(typeof(Func<string, FileInfo>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var fileInfo = container.Create()("C:\\Yeah.txt");
        Assert.Equal("C:\\Yeah.txt", fileInfo.FullName);
    }
}