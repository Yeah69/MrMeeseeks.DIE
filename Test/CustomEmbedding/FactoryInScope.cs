using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.FactoryInScope;

internal class ScopeRoot : IScopeRoot
{
    public string Property { get; }

    internal ScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal partial class Container
{
    partial class DIE_DefaultScope
    {
        private string DIE_Factory_Yeah() => "Yeah";
    }
}

public class Tests
{
    
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var scopeRoot = container.Create();
        Assert.Equal("Yeah", scopeRoot.Property);
    }
}