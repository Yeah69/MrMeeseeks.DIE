using MrMeeseeks.DIE.SampleChild;
using StrongInject;

namespace MrMeeseeks.DIE.Sample
{
    [Register(typeof(Context), typeof(IContext))]
    [Register(typeof(Child), typeof(Child), typeof(IChild))]
    [Register(typeof(InternalChild), typeof(IInternalChild))]
    internal partial class StrongInjectContainer : StrongInject.IContainer<IContext>
    {
        
    }
}