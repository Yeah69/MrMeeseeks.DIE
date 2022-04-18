using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.PropertyInScope;

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
        private string DIE_Yeah => "Yeah";
    }
}

public class Tests
{
    
    [Fact]
    public void Test()
    {
        var container = new Container();
        var scopeRoot = container.Create();
        Assert.Equal("Yeah", scopeRoot.Property);
    }
}