using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.Composite;

internal interface IInterface
{
    IReadOnlyList<IInterface> Implementations { get; }
}

internal class BaseA : IInterface
{
    public IReadOnlyList<IInterface> Implementations => new[] { this };
}

internal class BaseB : IInterface
{
    public IReadOnlyList<IInterface> Implementations => new[] { this };
}

internal class Composite<T0> : IInterface, IComposite<IInterface>
{
    public IReadOnlyList<IInterface> Implementations { get; }

    internal Composite(
        IReadOnlyList<IInterface> implementations) =>
        Implementations = implementations;
}

[GenericParameterChoice(typeof(Composite<>), "T0", typeof(int))]
[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container
{
    
}