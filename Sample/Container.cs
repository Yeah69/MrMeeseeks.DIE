using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface
{
    string Parameter { get; }
}

internal class Dependency : IInterface
{
    public required string Parameter { get; init; }
}

internal class Root
{
    public required IInterface Dependency { get; init; }
    
    internal Root()
    {
    }
}

[CreateFunction(typeof(Root), "Create", typeof(string))]
internal partial class Container
{
}