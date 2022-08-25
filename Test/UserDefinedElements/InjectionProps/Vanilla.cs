using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.Vanilla;

internal class Dependency
{
    public int Number { get; init; }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    [UserDefinedPropertiesInjection(typeof(Dependency))]
    private void DIE_Props_Dependency(out int Number) => Number = 69;
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}