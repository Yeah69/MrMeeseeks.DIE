using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IContainerInstance;

internal class Scope : IScopeRoot
{
    internal required Dependency Dependency { get; init; }
    internal required Dependency Dependency1 { get; init; }
}

internal class Parent
{
    internal required Scope Scope { get; init; }
    internal required Dependency Dependency { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
