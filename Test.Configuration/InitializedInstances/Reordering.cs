using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.Reordering;

internal sealed class DependencyA
{
    internal DependencyA(DependencyC c, DependencyB b) {}
}

internal sealed class DependencyB
{
    internal DependencyB(DependencyC c) {}
}

internal sealed class DependencyC;

internal sealed class Root : IScopeRoot
{
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
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