using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ThreadLocal.AsyncWrapped.SingleValueTask;

internal class Dependency : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => default;
}

internal class OuterDependency
{
    internal OuterDependency(
        // ReSharper disable once UnusedParameter.Local
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(ThreadLocal<ValueTask<OuterDependency>>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var lazy = container.Create();
        var _ = lazy.Value;
    }
}