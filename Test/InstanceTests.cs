using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface IInstanceClass
{
    string Dependency { get; }
}

internal class InstanceClass : IInstanceClass
{
    public string Dependency { get; }

    public InstanceClass(string dependency)
    {
        Dependency = dependency;
    }
}

[MultiContainer(typeof(IInstanceClass))]
internal partial class InstanceContainer
{
    private readonly string DIE_Dependency;

    public InstanceContainer(string dieDependency) => DIE_Dependency = dieDependency;
}

public partial class InstanceTests
{
    [Fact]
    public void ResolveExternalType()
    {
        var check = "Hello, instance!";
        using var container = new InstanceContainer(check);
        var instanceClass = container.Create0();
        Assert.Equal(check, instanceClass.Dependency);
    }
}