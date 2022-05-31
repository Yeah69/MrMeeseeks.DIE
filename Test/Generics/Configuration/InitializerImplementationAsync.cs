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

[TypeInitializer(typeof(Dependency<>), nameof(Dependency<int>.InitializeAsync))]
[CreateFunction(typeof(Dependency<int>), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = await container.CreateValueAsync().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}