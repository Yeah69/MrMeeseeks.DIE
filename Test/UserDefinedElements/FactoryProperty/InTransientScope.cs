using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.FactoryProperty.InTransientScope;

internal class TransientScopeRoot : ITransientScopeRoot
{
    public string Property { get; }

    internal TransientScopeRoot(string property) => Property = property;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    private sealed partial class DIE_DefaultTransientScope
    {
        private string DIE_Factory_Yeah => "Yeah";
    }
}

public class Tests
{
    
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.Equal("Yeah", transientScopeRoot.Property);
    }
}