using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal enum Keys
{
    A,
    B,
    C
}

internal interface IInterface { }

[Key(Keys.A)]
internal class DependencyA : IInterface { }

[Key(Keys.B)]
internal class DependencyB : IInterface { }

[Key(Keys.C)]
internal class DependencyC : IInterface { }

internal class Parent
{
    public List<IInterface> Dependency { get; }

    internal Parent([Key(Keys.C)] List<IInterface> dependency)
    {
        Dependency = dependency;
    }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    private Container() {}
}