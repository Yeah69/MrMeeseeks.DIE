using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.TaskComposition;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class DependencyA : ITaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyB : IValueTaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyC : IInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

internal class DependencyD : IInterface
{
    public bool IsInitialized => true;
}

internal class Composite : ITaskInitializer, IInterface, IComposite<IInterface>
{
    private readonly IReadOnlyList<Task<IInterface>> _composition;

    internal Composite(
        IReadOnlyList<Task<IInterface>> composition) =>
        _composition = composition;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_composition).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }

    public int Count => _composition.Count;
}
[CreateFunction(typeof(Task<IInterface>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.IsType<Composite>(instance);
        Assert.Equal(4, ((Composite) instance).Count);
        Assert.True(instance.IsInitialized);
    }
}