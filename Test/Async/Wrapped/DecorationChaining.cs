using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.DecorationChaining;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class Dependency : ITaskInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DecoratorA : IInitializer, IInterface, IDecorator<IInterface>
{
    private readonly Task<IInterface> _task;
    public bool IsInitialized { get; private set; }
    
    internal DecoratorA(Task<IInterface> task) => _task = task;

    void IInitializer.Initialize()
    {
        _task.Wait();
        IsInitialized = true;
    }
}

internal class DecoratorB : IValueTaskInitializer, IInterface, IDecorator<IInterface>
{
    public bool IsInitialized { get; private set; }
    
    internal DecoratorB(IInterface _) {}
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Task<IInterface>), "Create")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}