using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.ScopeUserDefinedAddForDisposalAsync;

internal sealed class Dependency : IAsyncDisposable
{
    internal bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return new ValueTask(Task.CompletedTask);
    }
}

internal sealed class ScopeRoot : IScopeRoot
{
    public Dependency Dependency { get; }

    internal ScopeRoot(Dependency dependency)
    {
        Dependency = dependency;
    }
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        // ReSharper disable once InconsistentNaming
        private Dependency DIE_Factory_Dependency
        {
            get
            {
                var dependency = new Dependency();
                DIE_AddForDisposalAsync(dependency);
                return dependency;
            }
        }

        private partial void DIE_AddForDisposalAsync(IAsyncDisposable asyncDisposable);
    }
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        await container.DisposeAsync();
        Assert.True(instance.Dependency.IsDisposed);
    }
}