using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.WithArrayParameter;

internal sealed class ParameterDependency;

internal sealed class Dependency
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(ParameterDependency[] parameters){}
}

[CreateFunction(typeof(Func<ParameterDependency[], Dependency>), "Create")]
[CreateFunction(typeof(ParameterDependency), "CreateParameter")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create()(new []
        {
            container.CreateParameter(), 
            container.CreateParameter(), 
            container.CreateParameter()
        });
    }
}
