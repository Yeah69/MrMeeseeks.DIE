using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer;

internal interface IInterface {}

internal class Dependency : IInterface, ITransientScopeInstance {}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Dependency>(instance);
    }
}