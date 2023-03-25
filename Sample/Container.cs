using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency
{
    internal int B { get; init; }    
    internal int? F { get; init; }

    internal Dependency(int a, int d, int? e) {}
    public void Initialize(int c) { }
}

internal class Root
{
    internal Root(Dependency _) { }
}

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}