using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

[InvocationDescription]
internal interface IInvocationDescription
{
    ITypeDescription TargetType { get; }
    IMethodDescription TargetMethod { get; }
}

[MethodDescription]
internal interface IMethodDescription
{
    string Name { get; }
    ITypeDescription ReturnType { get; }
}

[TypeDescription]
internal interface ITypeDescription
{
    string FullName { get; }
    string Name { get; }
}

internal interface IInterface<T0>;

internal sealed class Class<T0> : IInterface<T0>, IScopeRoot;

[CreateFunction(typeof(Class<>), "Create")]
[CreateFunction(typeof(IInterface<>), "CreateInterface")]
internal sealed partial class Container<T>
{
    private sealed partial class DIE_DefaultScope;
}

[CreateFunction(typeof(Class<>), "Create")]
[CreateFunction(typeof(IInterface<>), "CreateInterface")]
internal sealed partial class Container2<T>
{
    private sealed partial class DIE_DefaultScope;
}
