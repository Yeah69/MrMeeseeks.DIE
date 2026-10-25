using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class ErrorNonAsyncReturnTypeEntryFunction
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

    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public void Test() => 
            Assert.True(Container.DIE_CreateContainer().DieBuildErrorCodes is ["DIE_65_01"]);
    }
}