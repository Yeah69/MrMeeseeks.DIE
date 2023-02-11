using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IDisposable
{
    internal Dependency(Lazy<Root> _)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

internal class Root : IDisposable
{
    internal Root(Dependency _) {}

    public void Dispose()
    {
        throw new NotSupportedException();
    }
}

[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
}