using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface {}

internal class Dependency : ITransientScopeInstance, IValueTaskInitializer, IInterface
{
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Yield();
    }
}

internal class SyncDependency : IInterface
{
}

internal class OuterDependency : ITransientScopeInstance
{
    internal OuterDependency(IInterface _) {}
}

internal class Root : ITransientScopeRoot
{
    internal Root(ValueTask<OuterDependency> ___, Func<Dependency, ValueTask<OuterDependency>> __, Func<ValueTask<OuterDependency>> _)
    {
    }
}

[ImplementationChoice(typeof(IInterface), typeof(SyncDependency))]
[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
    [ImplementationChoice(typeof(IInterface), typeof(Dependency))]
    private partial class DIE_DefaultTransientScope {}
}