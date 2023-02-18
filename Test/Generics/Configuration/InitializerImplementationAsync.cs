using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.InitializerImplementationAsync;

internal class Dependency<T0>
{
    internal async Task InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }
}

[Initializer(typeof(Dependency<>), nameof(Dependency<int>.InitializeAsync))]
[CreateFunction(typeof(Dependency<int>), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}