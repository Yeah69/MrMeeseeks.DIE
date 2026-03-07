using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class OnlyScopeInScopeDependency;
internal class Dependency;
internal class ScopeDependency : IScopeInstance;
internal class TransientScopeDependency : ITransientScopeInstance;
internal class ContainerDependency : IContainerInstance;

internal class ScopeA
{
}

internal class ScopeB
{
}

internal class ScopeC
{
}

internal class TransientScopeA : ITransientScopeRoot
{
    internal required ScopeA Scope { get; init; }
}

internal class TransientScopeB : ITransientScopeRoot
{
    internal required ScopeB Scope { get; init; }
}

internal class TransientScopeC : ITransientScopeRoot
{
    internal required ScopeC Scope { get; init; }
}

internal class Parent
{
    internal required TransientScopeA TransientScopeA { get; init; }
    internal required TransientScopeB TransientScopeB { get; init; }
    internal required TransientScopeC TransientScopeC { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    [ScopeRootImplementationAggregation(typeof(ScopeA))]
    [CustomScopeForRootTypes(typeof(TransientScopeA))]
    private sealed partial class DIE_TransientScope_A;
    [ScopeRootImplementationAggregation(typeof(ScopeB))]
    [CustomScopeForRootTypes(typeof(TransientScopeB))]
    private sealed partial class DIE_TransientScope_B;
    [ScopeRootImplementationAggregation(typeof(ScopeC))]
    [CustomScopeForRootTypes(typeof(TransientScopeC))]
    private sealed partial class DIE_TransientScope_C;
    [CustomScopeForRootTypes(typeof(ScopeA))]
    private sealed partial class DIE_Scope_A;
    [CustomScopeForRootTypes(typeof(ScopeB))]
    private sealed partial class DIE_Scope_B;
    [CustomScopeForRootTypes(typeof(ScopeC))]
    private sealed partial class DIE_Scope_C;
}
