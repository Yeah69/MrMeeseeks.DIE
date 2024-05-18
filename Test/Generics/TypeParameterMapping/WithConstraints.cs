using System.Threading.Tasks;
using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.TypeParameterMapping.WithConstraints;

internal sealed class Dependency<T0, T1>
    where T0 : struct
    where T1 : class;

internal sealed class TypeArgument;

[CreateFunction(typeof(Dependency<,>), "Create")]
internal sealed partial class Container<
    [GenericParameterMapping(typeof(Dependency<,>), "T0")] TA,
    [GenericParameterMapping(typeof(Dependency<,>), "T1")] TB>
    // The container's type parameter has to have same constraints as the mapped type parameter
    where TA : struct
    // but it can also be more strictly constrained
    where TB : class, new();

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container<int, TypeArgument>.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency<int, TypeArgument>>(instance);
    }
}