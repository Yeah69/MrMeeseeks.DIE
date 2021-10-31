using MrMeeseeks.DIE;
using SampleChild;

[assembly:Spy(typeof(IPublicTypeReport), typeof(IInternalTypeReport))]

namespace MrMeeseeks.DIE.Sample
{
    internal partial class Container : IContainer<IContext>
    {
    }
}
