using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency;
internal class ScopeDependency : IScopeInstance;
internal class TransientScopeDependency : ITransientScopeInstance;
internal class ContainerDependency : IContainerInstance;

internal class Scope : IScopeRoot
{
    internal required Dependency Dependency { get; init; }
    internal required Dependency Dependency1 { get; init; }
    internal required ScopeDependency ScopeDependency { get; init; }
    internal required ScopeDependency ScopeDependency1 { get; init; }
    internal required TransientScopeDependency TransientScopeDependency { get; init; }
    internal required TransientScopeDependency TransientScopeDependency1 { get; init; }
    internal required ContainerDependency ContainerScopeDependency { get; init; }
    internal required ContainerDependency ContainerScopeDependency1 { get; init; }
}

internal class TransientScope : ITransientScopeRoot
{
    internal required Scope Scope { get; init; }
    internal required Dependency Dependency { get; init; }
    internal required Dependency Dependency1 { get; init; }
    internal required ScopeDependency ScopeDependency { get; init; }
    internal required ScopeDependency ScopeDependency1 { get; init; }
    internal required TransientScopeDependency TransientScopeDependency { get; init; }
    internal required TransientScopeDependency TransientScopeDependency1 { get; init; }
    internal required ContainerDependency ContainerScopeDependency { get; init; }
    internal required ContainerDependency ContainerScopeDependency1 { get; init; }
}

internal class Parent
{
    internal required TransientScope TransientScope { get; init; }
    internal required Scope Scope { get; init; }
    internal required Dependency Dependency { get; init; }
    internal required Dependency Dependency1 { get; init; }
    internal required ScopeDependency ScopeDependency { get; init; }
    internal required ScopeDependency ScopeDependency1 { get; init; }
    internal required TransientScopeDependency TransientScopeDependency { get; init; }
    internal required TransientScopeDependency TransientScopeDependency1 { get; init; }
    internal required ContainerDependency ContainerScopeDependency { get; init; }
    internal required ContainerDependency ContainerScopeDependency1 { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
