using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Lazy.AsyncWrapped.SingleTask;

internal class Dependency : IValueTaskInitializer
{
    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal class OuterDependency
{
    internal OuterDependency(
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(Lazy<Task<OuterDependency>>), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var lazy = container.Create();
        var _ = lazy.Value;
    }
}
