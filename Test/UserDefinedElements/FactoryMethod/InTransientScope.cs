using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryMethod.InTransientScope;

internal class TransientScopeRoot : ITransientScopeRoot
{
    public string Property { get; }

    internal TransientScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    private sealed partial class DIE_DefaultTransientScope
    {
        private string DIE_Factory_Yeah() => "Yeah";
    }
}

public class Tests
{
    
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.Equal("Yeah", transientScopeRoot.Property);
    }
}