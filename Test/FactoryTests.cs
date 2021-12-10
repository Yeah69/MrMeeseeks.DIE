using System.IO;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal partial class FactoryContainer : IContainer<FileInfo>
{
    private string DIE_Path { get; }

    private FileInfo DIE_FileInfo(string path) => new (path);

    public FactoryContainer(string diePath) => DIE_Path = diePath;
}

public partial class FactoryTests
{
    [Fact]
    public void ResolveExternalType()
    {
        var check = @"C:\HelloWorld.txt";
        using var container = new FactoryContainer(check);
        var fileInfo = ((IContainer<FileInfo>) container).Resolve();
        Assert.Equal(check, fileInfo.FullName);
    }
}