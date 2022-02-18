using System.IO;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal class FactoryContainerTransientScope : ITransientScopeRoot
{
    public int Number { get; }

    internal FactoryContainerTransientScope(int number)
    {
        Number = number;
    }
}
internal class FactoryContainerScope : IScopeRoot
{
    public string Yeah { get; }

    internal FactoryContainerScope(string yeah)
    {
        Yeah = yeah;
    }
}

[MultiContainer(typeof(FileInfo), typeof(FactoryContainerTransientScope), typeof(FactoryContainerScope))]
internal partial class FactoryContainer
{
    private string DIE_Path { get; }

    private FileInfo DIE_FileInfo(string path) => new (path);

    public FactoryContainer(string diePath) => DIE_Path = diePath;

    partial class DIE_DefaultTransientScope
    {
        private int DIE_Num => 69;
    }

    partial class DIE_DefaultScope
    {
        private string DIE_Yeah => "Yeah";
    }
}

public partial class FactoryTests
{
    [Fact]
    public void ResolveExternalType()
    {
        var check = @"C:\HelloWorld.txt";
        using var container = new FactoryContainer(check);
        var fileInfo = container.Create0();
        Assert.Equal(check, fileInfo.FullName);
    }
    
    [Fact]
    public void ResolveFromFactoryInTransientScope()
    {
        var check = @"C:\HelloWorld.txt";
        using var container = new FactoryContainer(check);
        var transientScope = container.Create1();
        Assert.Equal(69, transientScope.Number);
    }
    
    [Fact]
    public void ResolveFromFactoryInScope()
    {
        var check = @"C:\HelloWorld.txt";
        using var container = new FactoryContainer(check);
        var scope = container.Create2();
        Assert.Equal("Yeah", scope.Yeah);
    }
}