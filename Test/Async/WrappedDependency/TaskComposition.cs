using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.TaskComposition;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class DependencyA : ITaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyB : IValueTaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyC : ITypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

internal class DependencyD : IInterface
{
    public bool IsInitialized => true;
}

internal class Composite : ITaskTypeInitializer, IInterface, IComposite<IInterface>
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

[MultiContainer(typeof(Task<IInterface>))]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = await container.Create0().ConfigureAwait(false);
        Assert.IsType<Composite>(instance);
        Assert.Equal(4, ((Composite) instance).Count);
        Assert.True(instance.IsInitialized);
    }
}