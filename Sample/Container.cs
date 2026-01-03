using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IContainerInstance;

internal class Parent
{
    internal required Dependency DependencyA { get; init; }
    internal required Dependency DependencyB { get; init; }
    internal bool SameSame => ReferenceEquals(DependencyA, DependencyB);
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
