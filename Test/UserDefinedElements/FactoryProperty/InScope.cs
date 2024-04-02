using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryProperty.InScope;

internal sealed class ScopeRoot : IScopeRoot
{
    public string Property { get; }

    internal ScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        // ReSharper disable once InconsistentNaming
        private string DIE_Factory_Yeah => "Yeah";
    }
}

public sealed class Tests
{
    
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var scopeRoot = container.Create();
        Assert.Equal("Yeah", scopeRoot.Property);
    }
}