using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
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
        await Task.Delay(500);
        IsInitialized = true;
    }
}

internal class DependencyB : IValueTaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Delay(500);
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
        await Task.WhenAll(_composition);
        await Task.Delay(500);
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }

    public int Count => _composition.Count;
}
[CreateFunction(typeof(Task<IInterface>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create();
        Assert.IsType<Composite>(instance);
        Assert.Equal(4, ((Composite) instance).Count);
        Assert.True(instance.IsInitialized);
    }
}