using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IValueTaskInitializer, IScopeInstance
{
    public async ValueTask InitializeAsync() => await Task.Yield();
}

internal class Root
{
    internal Root(ValueTask<Dependency> _) {}
}

[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
}