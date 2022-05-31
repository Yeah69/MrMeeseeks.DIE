using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Record.PrimaryConstr;

internal record Dependency;

internal record Implementation(Dependency Dependency);

[CreateFunction(typeof(Implementation), "Create")]
internal partial class Container{}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Implementation>(instance);
        Assert.IsType<Dependency>(instance.Dependency);
    }
}