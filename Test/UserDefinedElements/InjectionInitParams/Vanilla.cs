using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionInitParams.Vanilla;

internal class Dependency
{
    public int Number { get; private set; }

    internal void Initialize(int number) => Number = number;
}

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    [UserDefinedInitializerParametersInjection(typeof(Dependency))]
    private void DIE_InitParams_Dependency(out int number) => number = 69;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}