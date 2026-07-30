using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class Simplest
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

    [CreateFunction(typeof(ValueTask<Dependency>), "CreateValueTask")]
    [CreateFunction(typeof(Task<Dependency>), "CreateTask")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task TestValueTask()
        {
            var container = Container.DIE_CreateContainer();
            var dependency = await container.CreateValueTask();
            Assert.True(dependency.IsInitialized);
        }
        
        [Fact]
        public async Task TestTask()
        {
            var container = Container.DIE_CreateContainer();
            var dependency = await container.CreateTask();
            Assert.True(dependency.IsInitialized);
        }
    }
}