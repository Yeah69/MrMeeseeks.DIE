using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Async.SyncToAsyncFuncError;

internal class Class : ITaskInitializer
{
    public Task InitializeAsync() => Task.CompletedTask;
}

[CreateFunction(typeof(Func<Class>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        Assert.True(container.DieBuildErrorCodes is ["DIE_65_01"]);
    }
}