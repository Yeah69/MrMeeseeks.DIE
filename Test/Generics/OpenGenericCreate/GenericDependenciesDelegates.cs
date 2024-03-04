using System;
using System.Threading;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.GenericDependenciesDelegates;

internal interface IInterface<T0>;

internal class Dependency<T0> : IInterface<T0>;

internal interface IInterface<T3, T4, T5>
{
    Lazy<Dependency<T5>> DependencyInit { get; }
    Func<Dependency<T4>> DependencyConstrParam { get; }
    ThreadLocal<Dependency<T3>>? DependencyInitParam { get; }
    Func<IInterface<T5>> InterfaceInit { get; }
    Lazy<IInterface<T4>> InterfaceConstrParam { get; }
    ThreadLocal<IInterface<T3>>? InterfaceInitParam { get; }
}

internal class DependencyHolder<T0, T1, T2> : IInterface<T2, T1, T0>
{
    public required Lazy<Dependency<T0>> DependencyInit { get; init; }
    public Func<Dependency<T1>> DependencyConstrParam { get; }
    public ThreadLocal<Dependency<T2>>? DependencyInitParam { get; private set; }
    public required Func<IInterface<T0>> InterfaceInit { get; init; }
    public Lazy<IInterface<T1>> InterfaceConstrParam { get; }
    public ThreadLocal<IInterface<T2>>? InterfaceInitParam { get; private set; }
    
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(
        Func<Dependency<T1>> dependencyConstrParam, 
        Lazy<IInterface<T1>> interfaceConstrParam)
    {
        DependencyConstrParam = dependencyConstrParam;
        InterfaceConstrParam = interfaceConstrParam;
    }

    internal void Initialize(ThreadLocal<Dependency<T2>> dependencyInitParam, ThreadLocal<IInterface<T2>> interfaceInitParam)
    {
        DependencyInitParam = dependencyInitParam;
        InterfaceInitParam = interfaceInitParam;
    }
}

[Initializer(typeof(DependencyHolder<,,>), "Initialize")]
[CreateFunction(typeof(IInterface<,,>), "CreateInterface")]
[CreateFunction(typeof(DependencyHolder<,,>), "CreateConcreteType")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.CreateConcreteType<string, object, int>();
        Assert.IsType<DependencyHolder<string, object, int>>(instance);
        Assert.IsType<Dependency<string>>(instance.DependencyInit.Value);
        Assert.IsType<Dependency<object>>(instance.DependencyConstrParam());
        Assert.IsType<Dependency<int>>(instance.DependencyInitParam?.Value);
        Assert.IsType<Dependency<string>>(instance.InterfaceInit());
        Assert.IsType<Dependency<object>>(instance.InterfaceConstrParam.Value);
        Assert.IsType<Dependency<int>>(instance.InterfaceInitParam?.Value);
        var interfaceInstance = container.CreateInterface<string, object, int>();
        Assert.IsType<DependencyHolder<int, object, string>>(interfaceInstance);
        Assert.IsType<Dependency<int>>(interfaceInstance.DependencyInit.Value);
        Assert.IsType<Dependency<object>>(interfaceInstance.DependencyConstrParam());
        Assert.IsType<Dependency<string>>(interfaceInstance.DependencyInitParam?.Value);
        Assert.IsType<Dependency<int>>(interfaceInstance.InterfaceInit());
        Assert.IsType<Dependency<object>>(interfaceInstance.InterfaceConstrParam.Value);
        Assert.IsType<Dependency<string>>(interfaceInstance.InterfaceInitParam?.Value);
    }
}