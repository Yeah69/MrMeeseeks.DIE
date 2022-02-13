using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.DecorationChaining;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class Dependency : ITaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DecoratorA : ITypeInitializer, IInterface, IDecorator<IInterface>
{
    private readonly Task<IInterface> _task;
    public bool IsInitialized { get; private set; }
    
    internal DecoratorA(Task<IInterface> task) => _task = task;

    void ITypeInitializer.Initialize()
    {
        _task.Wait();
        IsInitialized = true;
    }
}

internal class DecoratorB : IValueTaskTypeInitializer, IInterface, IDecorator<IInterface>
{
    public bool IsInitialized { get; private set; }
    
    internal DecoratorB(IInterface _) {}
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[DecoratorSequenceChoice(typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
internal partial class Container : IContainer<Task<IInterface>>
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = await ((IContainer<Task<IInterface>>) container).Resolve().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}