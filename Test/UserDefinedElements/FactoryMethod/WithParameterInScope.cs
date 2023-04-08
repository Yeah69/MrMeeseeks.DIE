using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.WithParameterInScope;

internal class ScopeRoot : IScopeRoot
{
    public FileInfo Property { get; }

    internal ScopeRoot(FileInfo property) => Property = property;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    private sealed partial class DIE_DefaultScope
    {
        private string DIE_Factory_Path => "C:\\Yeah.txt";
        private FileInfo DIE_Factory(string path) => new (path);
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var scopeRoot = container.Create();
        Assert.Equal("C:\\Yeah.txt", scopeRoot.Property.FullName);
    }
}