using System.IO;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FactoryWithParameterInTransientScope;

internal class TransientScopeRoot : ITransientScopeRoot
{
    public FileInfo Property { get; }

    internal TransientScopeRoot(FileInfo property) => Property = property;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal partial class Container
{
    partial class DIE_DefaultTransientScope
    {
        private string DIE_Factory_Path => "C:\\Yeah.txt";
        private FileInfo DIE_Factory(string path) => new (path);
    }
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.Equal("C:\\Yeah.txt", transientScopeRoot.Property.FullName);
    }
}