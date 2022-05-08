using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Test.Record.PrimaryConstr;

internal interface IInterface {}

internal class Implementation : IInterface {}

internal class ImplementationA : IInterface {}

internal class Dependency
{
    public IReadOnlyList<IInterface> Item { get; }

    internal Dependency(IReadOnlyList<IInterface> item)
    {
        Item = item;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container{}