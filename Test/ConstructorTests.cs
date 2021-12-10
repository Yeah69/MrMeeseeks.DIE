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

internal partial class ConstructorInitContainer : IContainer<IConstructorInit>
{
}

public partial class ConstructorsTests
{
    [Fact]
    public void ResolveInitProperty()
    {
        using var container = new ConstructorInitContainer();
        var resolution = ((IContainer<IConstructorInit>) container).Resolve();
        Assert.NotNull(resolution.Dependency);
        Assert.IsType<ConstructorInitDependency>(resolution.Dependency);
    }
}