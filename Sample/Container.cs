using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.Composite;

internal class DependencyHolder
{
}

[CreateFunction(typeof(DependencyHolder), "Create")]
internal sealed partial class Container
{
}