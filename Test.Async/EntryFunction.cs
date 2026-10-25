using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class EntryFunction
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
        internal required Dependency Dependency { get; init; }
    }

    [CreateFunction(typeof(ValueTask<Parent>), "CreateValueTask")]
    [CreateFunction(typeof(Task<Parent>), "CreateTask")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task TestValueTask()
        {
            var container = Container.DIE_CreateContainer();
            var parent = await container.CreateValueTask();
            Assert.True(parent.Dependency.IsInitialized);
        }
        
        [Fact]
        public async Task TestTask()
        {
            var container = Container.DIE_CreateContainer();
            var parent = await container.CreateTask();
            Assert.True(parent.Dependency.IsInitialized);
        }
    }
}