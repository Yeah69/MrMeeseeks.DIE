using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal sealed class DeeperClassA;

internal sealed class DeeperClassB
{
    internal required DeeperClassA DeeperClassA { get; init; }
    internal required long Long { get; init; }
}

internal sealed class Class
{
    internal required DeeperClassA DeeperClassA { get; init; }
    internal required DeeperClassB DeeperClassB { get; init; }
    internal required string String { get; init; }
    internal required int Int { get; init; }
}

[CreateFunction(typeof(Class), "Create", typeof(string), typeof(int), typeof(string), typeof(int), typeof(long))]
internal sealed partial class Container;
