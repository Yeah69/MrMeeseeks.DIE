using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration;

public sealed partial class Composite
{
    internal interface IInterface
    {
        IReadOnlyList<IInterface> Implementations { get; }
    }

    internal sealed class BaseA : IInterface
    {
        public IReadOnlyList<IInterface> Implementations => [this];
    }

    internal sealed class BaseB : IInterface
    {
        public IReadOnlyList<IInterface> Implementations => [this];
    }

    // ReSharper disable once UnusedTypeParameter
    internal sealed class CompositeType<T0> : IInterface, IComposite<IInterface>
    {
        public IReadOnlyList<IInterface> Implementations { get; }

        internal CompositeType(
            IReadOnlyList<IInterface> implementations) =>
            Implementations = implementations;
    }

    [GenericParameterChoice(typeof(CompositeType<>), "T0", typeof(int))]
    [CreateFunction(typeof(IInterface), "Create")]
    internal sealed partial class Container;

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            await using var container = Container.DIE_CreateContainer();
            var composite = container.Create();
            Assert.IsType<CompositeType<int>>(composite);
        }
    }
}
