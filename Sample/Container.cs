using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.Composite;

internal class Dependency{}

internal class DependencyA
{
    internal DependencyA(Dependency _){}
}

internal class DependencyHolder : IScopeRoot
{
    internal DependencyHolder(DependencyA _, Dependency __) {}
}

[CreateFunction(typeof(DependencyHolder), "Create")]
internal sealed partial class Container
{
    [CustomScopeForRootTypes(typeof(DependencyHolder))]
    [InitializedInstancesForScopes(typeof(Dependency), typeof(DependencyA))]
    private partial class DIE_Scope
    {
        
    }
}