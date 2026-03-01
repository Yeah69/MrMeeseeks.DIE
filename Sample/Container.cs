using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class OnlyScopeInScopeDependency;
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
    internal required OnlyScopeInScopeDependency OnlyScopeInScopeDependency { get; init; }
    internal required OnlyScopeInScopeDependency OnlyScopeInScopeDependency1 { get; init; }
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
    internal required OnlyScopeInScopeDependency OnlyScopeInScopeDependency { get; init; }
    internal required OnlyScopeInScopeDependency OnlyScopeInScopeDependency1 { get; init; }
}

internal class Parent
{
    internal required TransientScope TransientScope { get; init; }
    internal required TransientScope TransientScope1 { get; init; }
    internal required Scope Scope { get; init; }
    internal required Func<int, string, Dependency> Dependency { get; init; }
    internal required Dependency Dependency1 { get; init; }
    internal required ScopeDependency ScopeDependency { get; init; }
    internal required ScopeDependency ScopeDependency1 { get; init; }
    internal required TransientScopeDependency TransientScopeDependency { get; init; }
    internal required TransientScopeDependency TransientScopeDependency1 { get; init; }
    internal required ContainerDependency ContainerScopeDependency { get; init; }
    internal required ContainerDependency ContainerScopeDependency1 { get; init; }
    internal required OnlyScopeInScopeDependency OnlyScopeInScopeDependency { get; init; }
    internal required OnlyScopeInScopeDependency OnlyScopeInScopeDependency1 { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    [FilterContainerInstanceImplementationAggregation(typeof(ScopeDependency))]
    [ScopeInstanceImplementationAggregation(typeof(OnlyScopeInScopeDependency))]
    [ContainerInstanceImplementationAggregation(typeof(Dependency), typeof(ScopeDependency))]
    private sealed partial class DIE_DefaultScope;
    [ContainerInstanceImplementationAggregation(typeof(OnlyScopeInScopeDependency))]
    [ContainerInstanceImplementationAggregation(typeof(Dependency))]
    private sealed partial class DIE_DefaultTransientScope;
}
