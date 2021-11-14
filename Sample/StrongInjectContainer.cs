using MrMeeseeks.DIE.SampleChild;
using StrongInject;
using StrongInject.Modules;

namespace MrMeeseeks.DIE.Sample;

[Register(typeof(Context), Scope.SingleInstance, typeof(Context), typeof(IContext))] 
[Register(typeof(Child), Scope.SingleInstance, typeof(Child), typeof(IChild))]
[Register(typeof(InternalChild), typeof(IInternalChild))]
[Register(typeof(YetAnotherInternalChild), typeof(IYetAnotherInternalChild))]
[Register(typeof(AndThenSingleInstance), typeof(IAndThenSingleInstance))]
[Register(typeof(AndThenAnotherScope), typeof(IAndThenAnotherScope))]
[RegisterModule(typeof(StandardModule))]
internal partial class StrongInjectContainer : StrongInject.IContainer<IContext>
{
        
}