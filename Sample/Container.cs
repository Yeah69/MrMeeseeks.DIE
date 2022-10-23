using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.Bugs.ReuseOfFieldFactory;

internal interface IInterface<T> {}

internal class Dependency : IInterface<int> {}

internal class DependencyHolder
{
    public IInterface<int> Dependency { get; }
    internal DependencyHolder(IInterface<int> dependency)
    {
        Dependency = dependency;
    }
}

[CreateFunction(typeof(DependencyHolder), "CreateHolder")]
[CreateFunction(typeof(IInterface<int>), "CreateInterface")]
internal sealed partial class Container
{
    private readonly IInterface<int> DIE_Factory_dependency;

    internal Container(IInterface<int> dependency)
    {
        DIE_Factory_dependency = dependency;
    }
}