using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class IAsyncEnumerableAsyncItemsWrapped
{
    internal interface IDependency
    {
        bool IsInitialized { get; }
    }
    
    internal abstract class AsyncDependencyBase : IDependency, ITaskInitializer
    {
        public bool IsInitialized { get; private set; }
    
        async Task ITaskInitializer.InitializeAsync()
        {
            await Task.Yield();
            IsInitialized = true;
        }
    }

    internal sealed class AsyncDependencyA : AsyncDependencyBase;

    internal sealed class AsyncDependencyB : AsyncDependencyBase;

    internal sealed class AsyncDependencyC : AsyncDependencyBase;


    internal sealed class Parent
    {
        internal required IAsyncEnumerable<ValueTask<IDependency>> Dependencies { get; init; }
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
            await foreach(var dependency in parent.Dependencies)
                Assert.True((await dependency).IsInitialized);
                
        }
    }
}