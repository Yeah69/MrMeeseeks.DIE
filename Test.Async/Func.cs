using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class Func
{
    internal sealed class Dependency : ITaskInitializer
    {
        public bool IsInitialized { get; private set; }
    
        async Task ITaskInitializer.InitializeAsync()
        {
            await Task.Delay(500);
            IsInitialized = true;
        }
    }

    internal sealed class Parent
    {
        internal required Func<ValueTask<Dependency>> ValueTaskDependency { get; init; }
        internal required Func<Task<Dependency>> TaskDependency { get; init; }
    }

    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task TestValueTask()
        {
            var container = Container.DIE_CreateContainer();
            var parent = container.Create();
            Assert.True((await parent.ValueTaskDependency()).IsInitialized);
        }
        
        [Fact]
        public async Task TestTask()
        {
            var container = Container.DIE_CreateContainer();
            var parent = container.Create();
            Assert.True((await parent.TaskDependency()).IsInitialized);
        }
    }
}