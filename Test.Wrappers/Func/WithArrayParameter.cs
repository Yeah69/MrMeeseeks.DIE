using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.Test.Wrappers.Func.WithArrayParameter;

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
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        _ = container.Create()([container.CreateParameter(), container.CreateParameter(), container.CreateParameter()]);
    }
}
