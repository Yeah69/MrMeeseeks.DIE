using MrMeeseeks.DIE.SampleChild;
using StrongInject;
using StrongInject.Modules;

namespace MrMeeseeks.DIE.Sample
{
    [Register(typeof(Context), typeof(IContext))]
    [Register(typeof(Child), typeof(Child), typeof(IChild))]
    [Register(typeof(Child0), typeof(Child0), typeof(IChild))]
    [Register(typeof(Child1), typeof(Child1), typeof(IChild))]
    [Register(typeof(InternalChild), typeof(IInternalChild))]
    [Register(typeof(YetAnotherInternalChild), typeof(IYetAnotherInternalChild))]
    [RegisterModule(typeof(StandardModule))]
    internal partial class StrongInjectContainer : StrongInject.IContainer<IContext>
    {
        
    }
}