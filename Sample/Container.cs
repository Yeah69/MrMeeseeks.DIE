using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface<T0>;

internal sealed class Class<T0> : IInterface<T0>, IScopeRoot;

[CreateFunction(typeof(Class<>), "Create")]
[CreateFunction(typeof(IInterface<>), "CreateInterface")]
internal sealed partial class Container<T>
{
    private sealed partial class DIE_DefaultScope;
}
