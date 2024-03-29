using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryField.InTransientScope;

internal class TransientScopeRoot : ITransientScopeRoot
{
    public string Property { get; }

    internal TransientScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultTransientScope
    {
        // ReSharper disable once InconsistentNaming
        private readonly string DIE_Factory_Yeah = "Yeah";
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