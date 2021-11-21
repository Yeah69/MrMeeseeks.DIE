using MrMeeseeks.DIE.SampleChild;
using StrongInject;
using StrongInject.Modules;

namespace MrMeeseeks.DIE.Sample;
//*
[Register(typeof(Context), Scope.SingleInstance, typeof(Context), typeof(IContext))] 
[Register(typeof(Child), Scope.SingleInstance, typeof(Child), typeof(IChild))]
[Register(typeof(InternalChild), typeof(IInternalChild))]
[Register(typeof(YetAnotherInternalChild), typeof(IYetAnotherInternalChild))]
[Register(typeof(AndThenSingleInstance), Scope.SingleInstance, typeof(IAndThenSingleInstance))]
[Register(typeof(AndThenAnotherScope), typeof(IAndThenAnotherScope))]
[Register(typeof(A), typeof(IA))]
[Register(typeof(B), typeof(IB))]
[RegisterModule(typeof(StandardModule))]
internal partial class StrongInjectContainer : StrongInject.IContainer<IContext>
{
        
}//*/