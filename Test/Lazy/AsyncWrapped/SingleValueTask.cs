using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Lazy.AsyncWrapped.SingleValueTask;

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

[CreateFunction(typeof(Lazy<ValueTask<OuterDependency>>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var lazy = container.Create();
        var _ = lazy.Value;
    }
}
