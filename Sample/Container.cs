using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IContainerInstance;

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    public Dependency? FromFactory { get; private set; }
    private Dependency DIE_Factory()
    {
        FromFactory = new();
        return FromFactory;
    }
}