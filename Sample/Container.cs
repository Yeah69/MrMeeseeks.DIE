using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.Composite;

internal record struct Dependency<T0>(T0 Param);

internal class DependencyHolder<T0>
{
    public Dependency<T0> Dependency { get; set; }
    internal DependencyHolder(Dependency<T0> param) {}
}

[PropertyChoice(typeof(DependencyHolder<>), nameof(DependencyHolder<int>.Dependency))]
[CreateFunction(typeof(DependencyHolder<int>), "Create")]
internal sealed partial class Container
{
    
}