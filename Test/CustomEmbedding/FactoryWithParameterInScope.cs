using System.IO;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FactoryWithParameterInScope;

internal class ScopeRoot : IScopeRoot
{
    public FileInfo Property { get; }

    internal ScopeRoot(FileInfo property) => Property = property;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal partial class Container
{
    partial class DIE_DefaultScope
    {
        private string DIE_Path => "C:\\Yeah.txt";
        private FileInfo DIE_Factory(string path) => new (path);
    }
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var scopeRoot = container.Create();
        Assert.Equal("C:\\Yeah.txt", scopeRoot.Property.FullName);
    }
}