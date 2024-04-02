using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Collection.Injection.IAsyncEnumerable;

internal interface IInterface;

internal sealed class ClassA : IInterface, IValueTaskInitializer
{
    public ValueTask InitializeAsync() => new();
}

internal sealed class ClassB : IInterface, ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

internal sealed class ClassC : IInterface;

[CreateFunction(typeof(IAsyncEnumerable<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}