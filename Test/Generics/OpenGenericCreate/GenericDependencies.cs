using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.GenericDependencies;

internal interface IInterface<T0>;

internal class Dependency<T0> : IInterface<T0>;

internal interface IInterface<T3, T4, T5>
{
    
    Dependency<T5> DependencyInit { get; }
    Dependency<T4> DependencyConstrParam { get; }
    Dependency<T3>? DependencyInitParam { get; }
    IInterface<T5> InterfaceInit { get; init; }
    IInterface<T4> InterfaceConstrParam { get; }
    IInterface<T3>? InterfaceInitParam { get; }
}

internal class DependencyHolder<T0, T1, T2> : IInterface<T2, T1, T0>
{
    public required Dependency<T0> DependencyInit { get; init; }
    public Dependency<T1> DependencyConstrParam { get; }
    public Dependency<T2>? DependencyInitParam { get; private set; }
    public required IInterface<T0> InterfaceInit { get; init; }
    public IInterface<T1> InterfaceConstrParam { get; }
    public IInterface<T2>? InterfaceInitParam { get; private set; }
    
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(
        Dependency<T1> dependencyConstrParam, 
        IInterface<T1> interfaceConstrParam)
    {
        DependencyConstrParam = dependencyConstrParam;
        InterfaceConstrParam = interfaceConstrParam;
    }

    internal void Initialize(Dependency<T2> dependencyInitParam, IInterface<T2> interfaceInitParam)
    {
        DependencyInitParam = dependencyInitParam;
        InterfaceInitParam = interfaceInitParam;
    }
}

[Initializer(typeof(DependencyHolder<,,>), "Initialize")]
[CreateFunction(typeof(DependencyHolder<,,>), "Create")]
[CreateFunction(typeof(IInterface<,,>), "CreateInterface")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create<string, object, int>();
        Assert.IsType<DependencyHolder<string, object, int>>(instance);
        Assert.IsType<Dependency<string>>(instance.DependencyInit);
        Assert.IsType<Dependency<object>>(instance.DependencyConstrParam);
        Assert.IsType<Dependency<int>>(instance.DependencyInitParam);
        Assert.IsType<Dependency<string>>(instance.InterfaceInit);
        Assert.IsType<Dependency<object>>(instance.InterfaceConstrParam);
        Assert.IsType<Dependency<int>>(instance.InterfaceInitParam);
        var interfaceInstance = container.CreateInterface<string, object, int>();
        Assert.IsType<DependencyHolder<int, object, string>>(interfaceInstance);
        Assert.IsType<Dependency<int>>(interfaceInstance.DependencyInit);
        Assert.IsType<Dependency<object>>(interfaceInstance.DependencyConstrParam);
        Assert.IsType<Dependency<string>>(interfaceInstance.DependencyInitParam);
        Assert.IsType<Dependency<int>>(interfaceInstance.InterfaceInit);
        Assert.IsType<Dependency<object>>(interfaceInstance.InterfaceConstrParam);
        Assert.IsType<Dependency<string>>(interfaceInstance.InterfaceInitParam);
    }
}