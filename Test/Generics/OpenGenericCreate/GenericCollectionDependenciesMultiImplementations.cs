using System.Collections.Generic;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.GenericCollectionDependenciesMultiImplementations;

internal interface IInterface<T0> {}

internal class Dependency0<T0> : IInterface<T0> {}

internal class Dependency1<T0> : IInterface<T0> {}

internal interface IInterface<T3, T4, T5>
{
    
    IReadOnlyList<IInterface<T5>> DependencyInit { get; }
}

internal class DependencyHolder<T0, T1, T2> : IInterface<T2, T1, T0>
{
    public required IReadOnlyList<IInterface<T0>> DependencyInit { get; init; }
}

[CreateFunction(typeof(DependencyHolder<,,>), "Create")]
[CreateFunction(typeof(IInterface<,,>), "CreateInterface")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create<string, object, int>();
        Assert.Equal(2, instance.DependencyInit.Count);
        foreach (var dependency in instance.DependencyInit)
        {
            Assert.True(dependency is Dependency0<string> || dependency is Dependency1<string>);
        }
        var interfaceInstance = container.CreateInterface<string, object, int>();
        Assert.Equal(2, interfaceInstance.DependencyInit.Count);
        foreach (var dependency in interfaceInstance.DependencyInit)
        {
            Assert.True(dependency is Dependency0<int> || dependency is Dependency1<int>);
        }
    }
}