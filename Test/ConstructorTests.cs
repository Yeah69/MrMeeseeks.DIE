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

[CreateFunction(typeof(IConstructorInit), "Create")]
internal partial class ConstructorInitContainer
{
}

public class ConstructorsTests
{
    [Fact]
    public void ResolveInitProperty()
    {
        var container = new ConstructorInitContainer();
        var resolution = container.Create();
        Assert.NotNull(resolution.Dependency);
        Assert.IsType<ConstructorInitDependency>(resolution.Dependency);
    }
}