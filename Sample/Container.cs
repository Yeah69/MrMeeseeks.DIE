using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal sealed class DeeperClassA;

internal sealed class DeeperClassB
{
    internal required DeeperClassA DeeperClassA { get; init; }
}

internal class Class
{
    internal required DeeperClassA DeeperClassA { get; init; }
    internal required DeeperClassB DeeperClassB { get; init; }
}

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container;
