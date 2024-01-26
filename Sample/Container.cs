using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal interface InnerInterface<T> {}

internal class InnerClass<T> : InnerInterface<T> {}

internal interface IInterface<T, T0> where T :  InnerClass<InnerInterface<T0>>, InnerInterface<InnerInterface<T0>> {}

internal class Class<T, T0> : IInterface<T, T0> where T : InnerClass<InnerInterface<T0>> {}

[CreateFunction(typeof(IInterface<,>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}
//*/