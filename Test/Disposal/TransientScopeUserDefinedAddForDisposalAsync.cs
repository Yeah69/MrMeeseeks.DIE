using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeUserDefinedAddForDisposalAsync;

internal class Dependency : IAsyncDisposable
{
    internal bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return new ValueTask(Task.CompletedTask);
    }
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public Dependency Dependency { get; }

    internal TransientScopeRoot(Dependency dependency)
    {
        Dependency = dependency;
    }
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal partial class Container
{
    private partial class DIE_DefaultTransientScope
    {
        private Dependency DIE_Factory_Dependency
        {
            get
            {
                var dependency = new Dependency();
                DIE_AddForDisposalAsync(dependency);
                return dependency;
            }
        }

        private partial void DIE_AddForDisposalAsync(IAsyncDisposable disposable);
    }
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.True(instance.Dependency.IsDisposed);
    }
}