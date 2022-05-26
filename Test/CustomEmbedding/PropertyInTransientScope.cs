using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.PropertyInTransientScope;

internal class TransientScopeRoot : ITransientScopeRoot
{
    public string Property { get; }

    internal TransientScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal partial class Container
{
    partial class DIE_DefaultTransientScope
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
        var transientScopeRoot = container.Create();
        Assert.Equal("Yeah", transientScopeRoot.Property);
    }
}