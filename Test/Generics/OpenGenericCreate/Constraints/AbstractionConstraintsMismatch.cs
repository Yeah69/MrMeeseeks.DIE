using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.Constraints.AbstractionConstraintsMismatch;

internal interface IInterface;
internal class BaseClass : IInterface;

internal interface IInterface<T0>; 
internal class DependencyStruct<T1> : IInterface<T1> where T1 : struct;
internal class DependencyClass<T2> : IInterface<T2> where T2 : class;
internal class DependencyNullableClass<T3> : IInterface<T3> where T3 : class?;
internal class DependencyNotNull<T4> : IInterface<T4> where T4 : notnull;
internal class DependencyUnmanaged<T5> : IInterface<T5> where T5 : unmanaged;
internal class DependencyNew<T6> : IInterface<T6> where T6 : new();
internal class DependencyBaseClass<T6> : IInterface<T6> where T6 : BaseClass;
internal class DependencyNullableBaseClass<T7> : IInterface<T7> where T7 : BaseClass?;
internal class DependencyInterface<T8> : IInterface<T8> where T8 : IInterface;
internal class DependencyNullableInterface<T9> : IInterface<T9> where T9 : IInterface?;
internal sealed class DependencyControlGroup<T10> : IInterface<T10>;

internal sealed class Proxy<T>
{
    internal required IReadOnlyList<IInterface<T>> Dependencies { get; init; }
}

[CreateFunction(typeof(Proxy<>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create<int>();
        Assert.Single(instance.Dependencies);
        Assert.IsType<DependencyControlGroup<int>>(instance.Dependencies[0]);
    }
}