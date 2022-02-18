using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface IConstructorInitDependency {}

internal class ConstructorInitDependency : IConstructorInitDependency {}

internal interface IConstructorInit
{
    IConstructorInitDependency? Dependency { get; }
}

internal class ConstructorInit : IConstructorInit
{
    public IConstructorInitDependency? Dependency { get; init; }
}

[MultiContainer(typeof(IConstructorInit))]
internal partial class ConstructorInitContainer
{
}

public class ConstructorsTests
{
    [Fact]
    public void ResolveInitProperty()
    {
        using var container = new ConstructorInitContainer();
        var resolution = container.Create0();
        Assert.NotNull(resolution.Dependency);
        Assert.IsType<ConstructorInitDependency>(resolution.Dependency);
    }
}