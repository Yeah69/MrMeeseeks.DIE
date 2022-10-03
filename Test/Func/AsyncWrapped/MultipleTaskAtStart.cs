using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.AsyncWrapped.MultipleTaskAtStart;

internal class Dependency : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => default;
}

internal class OuterDependency
{
    internal OuterDependency(
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(Func<ValueTask<Task<ValueTask<Task<OuterDependency>>>>>), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var func = container.Create();
        var _ = func();
    }
}
