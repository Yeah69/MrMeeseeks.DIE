using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface;

public sealed partial class DoubleSwitched
{
    // ReSharper disable UnusedTypeParameter
    internal interface IInterface<T0, T1>;
    // ReSharper restore UnusedTypeParameter

    internal sealed class Class<T1, T0> : IInterface<T0, T1>;

    [CreateFunction(typeof(IInterface<int, string>), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var instance = container.Create();
            Assert.IsType<Class<string, int>>(instance);
        }
    }
}
