using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.AsReturnedType;

internal class Dependency { }

[InitializedInstances(typeof(Dependency))]
[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var referenceA = container.Create();
        var referenceB = container.Create();
        Assert.Same(referenceA, referenceB);
    }
}