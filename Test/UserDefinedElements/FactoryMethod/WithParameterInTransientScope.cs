using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.WithParameterInTransientScope;

internal class TransientScopeRoot : ITransientScopeRoot
{
    public FileInfo Property { get; }

    internal TransientScopeRoot(FileInfo property) => Property = property;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    private sealed partial class DIE_DefaultTransientScope
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
        using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.Equal("C:\\Yeah.txt", transientScopeRoot.Property.FullName);
    }
}