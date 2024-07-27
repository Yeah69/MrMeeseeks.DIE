using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.Awaited;

internal sealed class DependencyA : IInitializer
{
    public void Initialize()
    {
    }
}

internal sealed class DependencyB : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => 
        new();
}

internal sealed class DependencyC : ITaskInitializer
{
    public Task InitializeAsync() => 
        Task.CompletedTask;
}

internal sealed class Root : IScopeRoot
{
    // ReSharper disable UnusedParameter.Local
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
    // ReSharper restore UnusedParameter.Local
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    
    [InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope;
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        _ = container.Create();
    }
}