using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration;

public sealed partial class InitializerInterfaceSync
{
    // ReSharper disable once UnusedTypeParameter
    internal sealed class Dependency<T0> : IInitializer
    {
        void IInitializer.Initialize()
        {
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }
    }

    [CreateFunction(typeof(Dependency<int>), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var instance = container.Create();
            Assert.True(instance.IsInitialized);
        }
    }
}
