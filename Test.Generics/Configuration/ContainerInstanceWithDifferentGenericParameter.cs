using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration;

public sealed partial class ContainerInstanceWithDifferentGenericParameter
{
    // ReSharper disable once UnusedTypeParameter
    internal sealed class Class<T0> : IContainerInstance;

    [CreateFunction(typeof(Class<int>), "Create")]
    [CreateFunction(typeof(Class<string>), "CreateString")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var instance0 = container.Create();
            var instance1 = container.CreateString();
            Assert.NotSame(instance0, instance1);
        }
    }
}
