using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => new();
}

internal class Root : IScopeRoot
{
    internal Root(Func<Dependency> _){}
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}