using System.Collections.Generic;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.GenericDependenciesNestedTypeParameters;

internal interface IInterface<T1, T0> {}

internal class Dependency<T0, T1> : IInterface<T0, T1> {}

internal interface IInterface<T3, T4, T5>
{
    
    Dependency<IReadOnlyList<T5>, T3> DependencyInit { get; }
    Dependency<string, IReadOnlySet<T4>> DependencyConstrParam { get; }
    Dependency<IReadOnlyDictionary<T3, T4>, T5>? DependencyInitParam { get; }
    IInterface<T5, IReadOnlyDictionary<T3, int>> InterfaceInit { get; init; }
    IInterface<IReadOnlyDictionary<int, double>, T5> InterfaceConstrParam { get; }
    IInterface<byte, IReadOnlyDictionary<T3, T5>>? InterfaceInitParam { get; }
}

internal class DependencyHolder<T0, T1, T2> : IInterface<T2, T1, T0>
{
    public required Dependency<IReadOnlyList<T0>, T2> DependencyInit { get; init; }
    public Dependency<string, IReadOnlySet<T1>> DependencyConstrParam { get; }
    public Dependency<IReadOnlyDictionary<T2, T1>, T0>? DependencyInitParam { get; private set; }
    public required IInterface<T0, IReadOnlyDictionary<T2, int>> InterfaceInit { get; init; }
    public IInterface<IReadOnlyDictionary<int, double>, T0> InterfaceConstrParam { get; }
    public IInterface<byte, IReadOnlyDictionary<T2, T0>>? InterfaceInitParam { get; private set; }
    
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(
        Dependency<string, IReadOnlySet<T1>> dependencyConstrParam, 
        IInterface<IReadOnlyDictionary<int, double>, T0> interfaceConstrParam)
    {
        DependencyConstrParam = dependencyConstrParam;
        InterfaceConstrParam = interfaceConstrParam;
    }

    internal void Initialize(Dependency<IReadOnlyDictionary<T2, T1>, T0> dependencyInitParam, IInterface<byte, IReadOnlyDictionary<T2, T0>> interfaceInitParam)
    {
        DependencyInitParam = dependencyInitParam;
        InterfaceInitParam = interfaceInitParam;
    }
}

[Initializer(typeof(DependencyHolder<,,>), "Initialize")]
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
        Assert.IsType<DependencyHolder<string, object, int>>(instance);
        Assert.IsType<Dependency<IReadOnlyList<string>, int>>(instance.DependencyInit);
        Assert.IsType<Dependency<string, IReadOnlySet<object>>>(instance.DependencyConstrParam);
        Assert.IsType<Dependency<IReadOnlyDictionary<int, object>, string>>(instance.DependencyInitParam);
        Assert.IsType<Dependency<string, IReadOnlyDictionary<int, int>>>(instance.InterfaceInit);
        Assert.IsType<Dependency<IReadOnlyDictionary<int, double>, string>>(instance.InterfaceConstrParam);
        Assert.IsType<Dependency<byte, IReadOnlyDictionary<int, string>>>(instance.InterfaceInitParam);
        var interfaceInstance = container.CreateInterface<string, object, int>();
        Assert.IsType<DependencyHolder<int, object, string>>(interfaceInstance);
        Assert.IsType<Dependency<IReadOnlyList<int>, string>>(interfaceInstance.DependencyInit);
        Assert.IsType<Dependency<string, IReadOnlySet<object>>>(interfaceInstance.DependencyConstrParam);
        Assert.IsType<Dependency<IReadOnlyDictionary<string, object>, int>>(interfaceInstance.DependencyInitParam);
        Assert.IsType<Dependency<int, IReadOnlyDictionary<string, int>>>(interfaceInstance.InterfaceInit);
        Assert.IsType<Dependency<IReadOnlyDictionary<int, double>, int>>(interfaceInstance.InterfaceConstrParam);
        Assert.IsType<Dependency<byte, IReadOnlyDictionary<string, int>>>(interfaceInstance.InterfaceInitParam);
    }
}