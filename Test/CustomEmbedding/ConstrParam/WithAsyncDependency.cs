using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.ConstrParam.WithAsyncDependency;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal class OtherDependency : IValueTaskTypeInitializer
{
    public int Number => 69;
    public ValueTask InitializeAsync() => new (Task.CompletedTask);
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    [UserDefinedConstrParamsInjection(typeof(Dependency))]
    private void DIE_ConstrParam_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = await container.CreateAsync().ConfigureAwait(false);
        Assert.Equal(69, instance.Number);
    }
}