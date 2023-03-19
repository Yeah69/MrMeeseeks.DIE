using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

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
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    [InitializedInstancesForScopes(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    private sealed partial class DIE_DefaultScope
    {
        
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var _ = container.Create();
    }
}