using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Test.Nullability.MultipleConstructors;

internal class Dependency
{
    internal Dependency() {}
    internal Dependency(int _) {}
}

internal class Wrapper
{
    public Dependency? Dependency { get; }

    internal Wrapper(Dependency? dependency) => Dependency = dependency;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal partial class Container
{
    private int DIE_Counter() => 69;
}
