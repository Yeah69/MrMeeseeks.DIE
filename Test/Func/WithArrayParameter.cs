using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

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
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var _ = container.Create()(new []
        {
            container.CreateParameter(), 
            container.CreateParameter(), 
            container.CreateParameter()
        });
    }
}
