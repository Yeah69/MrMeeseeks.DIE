using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.WithArrayParameter;

internal class ParameterDependency {}

internal class Dependency
{
    internal Dependency(ParameterDependency[] parameters){}
}

[CreateFunction(typeof(Func<ParameterDependency[], Dependency>), "Create")]
[CreateFunction(typeof(ParameterDependency), "CreateParameter")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
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
