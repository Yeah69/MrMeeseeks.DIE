using System.Threading.Tasks;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.TransientScopeImplicitWithGenericContainer;

internal sealed class Class<T0> : ITransientScopeRoot;

[CreateFunction(typeof(Class<>), "Create")]
internal sealed partial class Container<T>;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container<int>.DIE_CreateContainer();
        var instance = container.Create<string>();
        Assert.IsType<Class<string>>(instance);
    }
}