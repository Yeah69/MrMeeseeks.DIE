using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeUserDefinedAddForDisposal;

internal class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
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
internal sealed partial class Container
{
    private Container() {}
    
    private sealed partial class DIE_DefaultTransientScope
    {
        private Dependency DIE_Factory_Dependency
        {
            get
            {
                var dependency = new Dependency();
                DIE_AddForDisposal(dependency);
                return dependency;
            }
        }

        private partial void DIE_AddForDisposal(IDisposable disposable);
    }
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var root = container.Create();
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.True(root.Dependency.IsDisposed);
    }
}