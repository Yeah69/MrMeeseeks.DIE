using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;
using MrMeeseeks.DIE.SampleChild;
using SampleChild;

[assembly:Spy(typeof(IPublicTypeReport), typeof(IInternalTypeReport))]
[assembly:Transient(typeof(IChild), typeof(Context), typeof(IInternalChild), typeof(IYetAnotherInternalChild))]

namespace MrMeeseeks.DIE.Sample
{
    internal partial class Container : IContainer<IContext>
    {
    }
}
