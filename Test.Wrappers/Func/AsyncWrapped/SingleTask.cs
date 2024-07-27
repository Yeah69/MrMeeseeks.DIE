using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.Test.Wrappers.Func.AsyncWrapped.SingleTask;

internal sealed class Dependency : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => default;
}

internal sealed class OuterDependency
{
    internal OuterDependency(
        // ReSharper disable once UnusedParameter.Local
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(Func<Task<OuterDependency>>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var func = container.Create();
        _ = func();
    }
}
