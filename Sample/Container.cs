using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface<T0>
{
    IReadOnlyList<IInterface<T0>> Implementations { get; }
}

internal sealed class BaseA<T0> : IInterface<T0>
{
    public IReadOnlyList<IInterface<T0>> Implementations => [this];
}

internal sealed class BaseB<T0> : IInterface<T0>
{
    public IReadOnlyList<IInterface<T0>> Implementations => [this];
}

// ReSharper disable once UnusedTypeParameter
internal sealed class Composite<T0, T1> : IInterface<T0>, IComposite<IInterface<T0>>
{
    public IReadOnlyList<IInterface<T0>> Implementations { get; }

    internal Composite(
        IReadOnlyList<IInterface<T0>> implementations) =>
        Implementations = implementations;
}

[GenericParameterChoice(typeof(Composite<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container;
