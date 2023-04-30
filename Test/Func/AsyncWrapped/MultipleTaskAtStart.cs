using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.AsyncWrapped.MultipleTaskAtStart;

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

[CreateFunction(typeof(Func<ValueTask<Task<ValueTask<Task<OuterDependency>>>>>), "Create")]
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
        var func = container.Create();
        var _ = func();
    }
}
