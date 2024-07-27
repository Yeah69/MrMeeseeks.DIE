using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class WrappingSync
{
    internal sealed class Dependency : IInitializer
    {
        public bool IsInitialized { get; private set; }
    
        void IInitializer.Initialize() => 
            IsInitialized = true;
    }

    internal sealed class Parent
    {
        internal required Task<Dependency> Dep1T { get; init; }
        internal required ValueTask<Dependency> Dep1Vt { get; init; }
        internal required ValueTask<Task<Dependency>> Dep2TVt { get; init; }
        internal required Task<ValueTask<Dependency>> Dep2VtT { get; init; }
        internal required Task<Task<Dependency>> Dep2TT { get; init; }
        internal required ValueTask<ValueTask<Dependency>> Dep2VtVt { get; init; }
        internal required Task<ValueTask<Task<Dependency>>> Dep3TVtT { get; init; }
        internal required ValueTask<Task<ValueTask<Dependency>>> Dep3VtTVt { get; init; }
        internal required Task<Task<Task<Dependency>>> Dep3TTT { get; init; }
        internal required ValueTask<ValueTask<ValueTask<Dependency>>> Dep3VtVtVt { get; init; }
    }

    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            var container = Container.DIE_CreateContainer();
            var parent = container.Create();
            Assert.True((await parent.Dep1T).IsInitialized);
            Assert.True((await parent.Dep1Vt).IsInitialized);
            Assert.True((await await parent.Dep2TVt).IsInitialized);
            Assert.True((await await parent.Dep2VtT).IsInitialized);
            Assert.True((await await parent.Dep2TT).IsInitialized);
            Assert.True((await await parent.Dep2VtVt).IsInitialized);
            Assert.True((await await await parent.Dep3TVtT).IsInitialized);
            Assert.True((await await await parent.Dep3VtTVt).IsInitialized);
            Assert.True((await await await parent.Dep3TTT).IsInitialized);
            Assert.True((await await await parent.Dep3VtVtVt).IsInitialized);
        }
    }
}