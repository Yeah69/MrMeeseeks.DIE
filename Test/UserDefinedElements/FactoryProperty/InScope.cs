using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryProperty.InScope;

internal class ScopeRoot : IScopeRoot
{
    public string Property { get; }

    internal ScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    private sealed partial class DIE_DefaultScope
    {
        private string DIE_Factory_Yeah => "Yeah";
    }
}

public class Tests
{
    
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var scopeRoot = container.Create();
        Assert.Equal("Yeah", scopeRoot.Property);
    }
}