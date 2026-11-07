using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.TypeParameterMapping;

public sealed partial class Vanilla
{
    internal sealed class Dependency<T0>;

    [CreateFunction(typeof(Dependency<>), "Create")]
    internal sealed partial class Container<[GenericParameterMapping(typeof(Dependency<>), "T0")] TA>;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container<int>.DIE_CreateContainer();
            var instance = container.Create();
            Assert.IsType<Dependency<int>>(instance);
        }
    }
}
