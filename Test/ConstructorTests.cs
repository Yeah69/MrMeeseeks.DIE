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

[CreateFunction(typeof(IConstructorInit), "CreateDep")]
internal partial class ConstructorInitContainer
{
}

public class ConstructorsTests
{
    [Fact]
    public void ResolveInitProperty()
    {
        using var container = new ConstructorInitContainer();
        var resolution = container.CreateDep();
        Assert.NotNull(resolution.Dependency);
        Assert.IsType<ConstructorInitDependency>(resolution.Dependency);
    }
}