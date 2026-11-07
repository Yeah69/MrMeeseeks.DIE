using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Implementation;

public sealed partial class Single
{
    // ReSharper disable once UnusedTypeParameter
    internal sealed class Class<T0>;

    [CreateFunction(typeof(Class<int>), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var instance = container.Create();
            Assert.IsType<Class<int>>(instance);
        }
    }
}
