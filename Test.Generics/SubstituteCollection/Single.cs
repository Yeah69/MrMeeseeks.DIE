using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.SubstituteCollection;

public sealed partial class Single
{
    internal interface IInterface;

    // ReSharper disable once UnusedTypeParameter
    internal sealed class Class<T0> : IInterface;

    [GenericParameterSubstitutesChoice(typeof(Class<>), "T0", typeof(int))]
    [GenericParameterChoice(typeof(Class<>), "T0", typeof(string))]
    [CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var list = container.Create();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, i => i is Class<int>);
            Assert.Contains(list, i => i is Class<string>);
        }
    }
}
