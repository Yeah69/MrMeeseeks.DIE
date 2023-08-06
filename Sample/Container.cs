using System;
using System.Threading;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency
{
    internal Dependency()
    {
        Value = "Thread" + Thread.CurrentThread.ManagedThreadId;
    }

    public string Value { get; set; }
}

internal class Root
{
    internal Root(ThreadLocal<Lazy<Dependency>> dependency) => Dependency = dependency;
    
    internal ThreadLocal<Lazy<Dependency>> Dependency { get; }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}