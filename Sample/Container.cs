using System.Collections.Generic;
using System.Threading.Tasks;
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

[CreateFunction(typeof(IReadOnlyDictionary<Keys, IInterface>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}