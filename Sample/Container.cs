using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

public sealed partial class Double
{
    // ReSharper disable once UnusedTypeParameter
    internal interface IInterface<T0>;

    // ReSharper disable once UnusedTypeParameter
    internal sealed class Class<T0, T1> : IInterface<T0>;

    [GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(int), typeof(string))]
    [CreateFunction(typeof(IReadOnlyList<IInterface<int>>), "Create")]
    internal sealed partial class Container;
}
