using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration;

public sealed partial class TransientScopeRoot
{
    internal sealed class ScopeInstance : IScopeInstance;

    // ReSharper disable once UnusedTypeParameter
    internal sealed class TransientScopeRootType<T0> : ITransientScopeRoot
    {
        public ScopeInstance ScopeInstance { get; }

        internal TransientScopeRootType(ScopeInstance scopeInstance) => ScopeInstance = scopeInstance;
    }

    internal sealed class Root
    {
        public TransientScopeRootType<int> TransientScopeRoot { get; }
        public ScopeInstance ScopeInstance { get; }

        internal Root(
            TransientScopeRootType<int> transientScopeRoot,
            ScopeInstance scopeInstance)
        {
            TransientScopeRoot = transientScopeRoot;
            ScopeInstance = scopeInstance;
        }
    }

    [CreateFunction(typeof(Root), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var root = container.Create();
            Assert.NotSame(root.ScopeInstance, root.TransientScopeRoot.ScopeInstance);
        }
    }
}
