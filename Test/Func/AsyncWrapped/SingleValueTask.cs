using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.AsyncWrapped.SingleValueTask;

internal class Dependency : IValueTaskTypeInitializer
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

[CreateFunction(typeof(Func<ValueTask<OuterDependency>>), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var func = container.Create();
        var _ = func();
    }
}
