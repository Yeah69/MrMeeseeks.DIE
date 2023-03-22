using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionInitParams.WithAsyncDependency;

internal class Dependency
{
    public int Number { get; private set; }

    internal void Initialize(int number) => Number = number;
}

internal class OtherDependency : IValueTaskInitializer
{
    public int Number => 69;
    public ValueTask InitializeAsync() => new (Task.CompletedTask);
}

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    [UserDefinedInitializerParametersInjection(typeof(Dependency))]
    private void DIE_InitParams_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.Equal(69, instance.Number);
    }
}