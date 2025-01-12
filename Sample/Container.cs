using System;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal sealed class DeeperClassA;

internal sealed class DeeperClassB
{
    internal required DeeperClassA DeeperClassA { get; init; }
}

internal sealed class Class
{
    internal required DeeperClassA DeeperClassA { get; init; }
    internal required Func<long, int, DeeperClassB> DeeperClassB { get; init; }
    internal required (DeeperClassB, DeeperClassA, DeeperClassB, DeeperClassA, DeeperClassB, DeeperClassA, DeeperClassB, DeeperClassA) Tuple { get; init; }
}

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container;
