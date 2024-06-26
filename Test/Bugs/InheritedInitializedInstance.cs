﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Bugs.InheritedInitializedInstance;

internal interface IInterface;

internal sealed class DependencyDeepClass;
internal sealed class DependencyClass;
internal sealed class DependencyDeepInterface;
internal sealed class DependencyInterface;

[InitializedInstances(typeof(DependencyDeepInterface))]
internal interface IContainerConfig1;

[InitializedInstances(typeof(DependencyInterface))]
internal interface IContainerConfig0 : IContainerConfig1;

[InitializedInstances(typeof(DependencyDeepClass))]
internal abstract class ContainerConfig1;

[InitializedInstances(typeof(DependencyClass))]
internal abstract class ContainerConfig0 : ContainerConfig1;

[CreateFunction(typeof(DependencyDeepClass), "CreateDeepClass")]
[CreateFunction(typeof(DependencyClass), "CreateClass")]
[CreateFunction(typeof(DependencyDeepInterface), "CreateDeepInterface")]
[CreateFunction(typeof(DependencyInterface), "CreateInterface")]
internal sealed partial class Container : ContainerConfig0, IContainerConfig0;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var deepClass = container.CreateDeepClass();
        Assert.Same(deepClass, container.CreateDeepClass());
        var @class = container.CreateClass();
        Assert.Same(@class, container.CreateClass());
        var deepInterface = container.CreateDeepInterface();
        Assert.Same(deepInterface, container.CreateDeepInterface());
        var @interface = container.CreateInterface();
        Assert.Same(@interface, container.CreateInterface());
    }
}