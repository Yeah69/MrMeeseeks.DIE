using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Choice;

public sealed partial class Single
{
    internal interface IInterface;

    // ReSharper disable once UnusedTypeParameter
    internal sealed class Class<T0> : IInterface;

    [GenericParameterChoice(typeof(Class<>), "T0", typeof(int))]
    [CreateFunction(typeof(IInterface), "Create")]
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
