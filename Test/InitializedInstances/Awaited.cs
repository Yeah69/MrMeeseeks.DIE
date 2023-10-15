using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.Awaited;

internal class DependencyA : IInitializer
{
    public void Initialize()
    {
    }
}

internal class DependencyB : IValueTaskInitializer
{
    public ValueTask InitializeAsync() => 
        new();
}

internal class DependencyC : ITaskInitializer
{
    public Task InitializeAsync() => 
        Task.CompletedTask;
}

internal class Root : IScopeRoot
{
    // ReSharper disable UnusedParameter.Local
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
    // ReSharper restore UnusedParameter.Local
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    [InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}