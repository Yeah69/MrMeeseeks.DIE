using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.ScopeUserDefinedAddForDisposal;

internal class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal class ScopeRoot : IScopeRoot
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
    private sealed partial class DIE_DefaultScope
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
    public async ValueTask Test()
    {
        await using var container = new Container();
        var root = container.Create();
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.True(root.Dependency.IsDisposed);
    }
}